#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Administration tools for BlazOrbit releases
    STRATEGY: Squash + Rebase, linear history

.DESCRIPTION
    Script for project administrators/maintainers.
    Manages releases, verifies PR quality, and maintains linear flow.

.EXAMPLE
    ./admin-tools.ps1 status
    Shows branch status, pending commits, versions

.EXAMPLE
    ./admin-tools.ps1 check-pr branch-name
    Verifies if PR meets requirements (1 commit, rebase done)

.EXAMPLE
    ./admin-tools.ps1 rc 1.0.0
    Creates release candidate branch

.EXAMPLE
    ./admin-tools.ps1 release 1.0.0
    Publishes stable release (merge to master + tag)

.NOTES
    Author: Samuel Maícas (@cdcsharp)
    Version: 2.0.0 - Squash+Rebase Strategy
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("status", "check-pr", "rc", "release", "hotfix", "changelog", "cleanup")]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Name,

    [Parameter()]
    [switch]$Force,

    [Parameter()]
    [switch]$DryRun
)

# Configuration
$Config = @{
    Remote = "origin"
    DevelopBranch = "develop"
    MainBranch = "master"
}

# Colors
$Colors = @{
    Success = "Green"
    Warning = "Yellow"
    Error = "Red"
    Info = "Cyan"
    Emphasis = "Magenta"
}

#region Helper Functions

function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor $Colors.Emphasis
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $Colors.Success
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Colors.Warning
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $Colors.Error
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Colors.Info
}

function Test-GitRepository {
    try {
        $null = git rev-parse --git-dir 2>$null
        return $true
    }
    catch {
        return $false
    }
}

function Get-LastTag {
    try {
        $tag = git describe --tags --abbrev=0 --match "v[0-9]*.[0-9]*.[0-9]*" $Config.MainBranch 2>$null
        if ($tag) { return $tag }
    }
    catch { }
    return "v0.0.0"
}

function Get-RepositoryInfo {
    $repoUrl = git remote get-url $Config.Remote 2>$null
    if ($repoUrl -match "github.com[:/](.+?)/(.+?)(?:\.git)?$") {
        return @{
            Owner = $matches[1]
            Repo = $matches[2]
        }
    }
    return $null
}

function Get-GitHubToken {
    # Try gh CLI first
    try {
        $token = & gh auth token 2>$null
        if ($token) { return $token }
    }
    catch { }

    # Fall back to environment variable
    if ($env:GITHUB_TOKEN) {
        return $env:GITHUB_TOKEN
    }

    return $null
}

function Invoke-GitHubApi {
    param(
        [string]$Endpoint,
        [string]$Method = "GET",
        [object]$Body = $null
    )

    $token = Get-GitHubToken
    if (-not $token) {
        Write-Warning "No GitHub token available. Install gh CLI and authenticate, or set GITHUB_TOKEN environment variable."
        return $null
    }

    $repo = Get-RepositoryInfo
    if (-not $repo) {
        Write-Warning "Could not determine GitHub repository from remote."
        return $null
    }

    $url = "https://api.github.com$Endpoint"
    $headers = @{
        "Authorization" = "Bearer $token"
        "Accept" = "application/vnd.github+json"
        "X-GitHub-Api-Version" = "2022-11-28"
    }

    try {
        $params = @{
            Uri = $url
            Method = $Method
            Headers = $headers
        }
        if ($Body -and $Method -ne "GET") {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            $headers["Content-Type"] = "application/json"
        }

        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Warning "GitHub API call failed: $_"
        return $null
    }
}

function Get-CommitsSinceTag {
    param([string]$Tag)
    $count = git rev-list --count "$Tag..HEAD" 2>$null
    if ($count) { return [int]$count }
    return 0
}

function Invoke-GitCommand {
    param(
        [string]$Command,
        [string[]]$Arguments,
        [switch]$IgnoreError
    )
    
    try {
        $output = & git $Command @Arguments 2>&1
        if ($LASTEXITCODE -ne 0 -and -not $IgnoreError) {
            Write-Error "git $Command failed: $output"
            return $false
        }
        # Some git commands succeed with empty output (fetch with no new refs,
        # rebase already up-to-date, etc). Empty output is success here, not failure.
        # Callers checking truthiness via `if (-not $result)` would otherwise exit
        # silently because $LASTEXITCODE was 0 so no Write-Error fired.
        if ([string]::IsNullOrEmpty([string]$output)) {
            return $true
        }
        return $output
    }
    catch {
        if (-not $IgnoreError) {
            Write-Error "git $Command failed: $_"
        }
        return $false
    }
}

function Test-BranchMergedViaPR {
    <#
    .SYNOPSIS
    Returns $true if the branch was merged via a closed-merged GitHub PR.
    Used as a fallback for `git branch --merged`, which cannot detect
    squash-merges because the squash commit on develop has a different SHA
    than the branch's commits. Best-effort: requires `gh` CLI and returns
    $false silently if unavailable.
    #>
    param([string]$Branch)
    try {
        $jsonResult = & gh pr list --state merged --head $Branch --json number 2>$null
        if ($LASTEXITCODE -ne 0) { return $false }
        if (-not $jsonResult) { return $false }
        $parsed = $jsonResult | ConvertFrom-Json
        return @($parsed).Count -gt 0
    }
    catch {
        return $false
    }
}

function Get-NextVersion {
    param([string]$LastTag)

    # Parse current version
    $version = $LastTag -replace 'v', ''
    $parts = $version -split '\.'
    $major = [int]$parts[0]
    $minor = [int]$parts[1]
    $patch = [int]$parts[2]

    $hasPublicApiChanges = $false

    # Try to detect public API changes via GitHub PR labels
    $repo = Get-RepositoryInfo
    if ($repo -and (Get-GitHubToken)) {
        Write-Info "Querying GitHub API for merged PR labels..."

        # Get merge commits since last tag on develop
        $mergeCommits = git log "$LastTag..$($Config.Remote)/$($Config.DevelopBranch)" --merges --format="%H" 2>$null

        if ($mergeCommits) {
            foreach ($commit in $mergeCommits) {
                $commit = $commit.Trim()
                if (-not $commit) { continue }

                # Get PRs associated with this merge commit
                $prs = Invoke-GitHubApi -Endpoint "/repos/$($repo.Owner)/$($repo.Repo)/commits/$commit/pulls"

                if ($prs) {
                    foreach ($pr in $prs) {
                        if ($pr.state -eq "closed" -and $pr.merged_at) {
                            # Check labels
                            $labels = Invoke-GitHubApi -Endpoint "/repos/$($repo.Owner)/$($repo.Repo)/issues/$($pr.number)/labels"
                            if ($labels) {
                                $labelNames = $labels | ForEach-Object { $_.name }
                                if ($labelNames -contains "changes/public-api") {
                                    $hasPublicApiChanges = $true
                                    Write-Info "  Found PR #$($pr.number) with 'changes/public-api' label"
                                    break
                                }
                            }
                        }
                    }
                }

                if ($hasPublicApiChanges) { break }
            }
        }

        if (-not $hasPublicApiChanges) {
            Write-Info "No public API changes detected in merged PRs"
        }
    }
    else {
        Write-Warning "GitHub API not available. Falling back to commit message heuristics."
        $commits = git log "$LastTag..$($Config.Remote)/$($Config.DevelopBranch)" --format="%H" 2>$null
        foreach ($commit in $commits) {
            $message = git log -1 --format="%B" $commit 2>$null
            if ($message -match "public.api|PublicAPI|breaking.change") {
                $hasPublicApiChanges = $true
                break
            }
        }
    }

    # Version bump logic
    if ($hasPublicApiChanges) {
        $minor++
        $patch = 0
        $bumpType = "MINOR"
    }
    else {
        $patch++
        $bumpType = "PATCH"
    }

    return @{
        Version = "$major.$minor.$patch"
        BumpType = $bumpType
        HasPublicApiChanges = $hasPublicApiChanges
    }
}

function Show-Status {
    Write-Header "Repository Status"
    
    # Version info
    $lastTag = Get-LastTag
    Write-Info "Last tag on master: $lastTag"
    
    $nextVersion = Get-NextVersion -LastTag $lastTag
    Write-Info "Next version: $($nextVersion.Version) ($($nextVersion.BumpType) bump)"
    if ($nextVersion.HasPublicApiChanges) {
        Write-Warning "Public API changes detected - will trigger MINOR bump"
    }
    
    $commitsSince = Get-CommitsSinceTag -Tag $lastTag
    Write-Info "Commits since $lastTag`: $commitsSince"
    
    # Branch status
    Write-Host "`n--- Main Branches ---" -ForegroundColor $Colors.Emphasis
    
    foreach ($branch in @($Config.MainBranch, $Config.DevelopBranch)) {
        $exists = git ls-remote --heads $Config.Remote $branch 2>$null
        if ($exists) {
            $ahead = git rev-list --count "$($Config.Remote)/$branch..$branch" 2>$null
            $behind = git rev-list --count "$branch..$($Config.Remote)/$branch" 2>$null
            
            $status = if ($ahead -gt 0) { "+$ahead local" }
                     elseif ($behind -gt 0) { "-$behind remote" }
                     else { "✓ sync" }
            
            $color = if ($ahead -gt 0 -or $behind -gt 0) { $Colors.Warning } else { $Colors.Success }
            Write-Host "$branch`: " -NoNewline
            Write-Host $status -ForegroundColor $color
        }
    }
    
    # Feature/release/hotfix branches
    Write-Host "`n--- Active Branches ---" -ForegroundColor $Colors.Emphasis
    $featureBranches = git branch -r --list "$($Config.Remote)/feature/*" "$($Config.Remote)/fix/*" 2>$null
    $releaseBranches = git branch -r --list "$($Config.Remote)/release/*" "$($Config.Remote)/hotfix/*" 2>$null
    
    if ($featureBranches) {
        Write-Host "Features/Fixes:"
        $featureBranches | ForEach-Object { Write-Host "  $_" }
    }
    
    if ($releaseBranches) {
        Write-Host "Releases/Hotfixes:"
        $releaseBranches | ForEach-Object { Write-Host "  $_" }
    }
    
    if (-not $featureBranches -and -not $releaseBranches) {
        Write-Info "No active branches"
    }
    
    # Check PRs ready to merge
    Write-Host "`n--- PRs Ready to Merge ---" -ForegroundColor $Colors.Emphasis
    $repoUrl = git remote get-url $Config.Remote 2>$null
    if ($repoUrl -match "github.com[:/](.+)/(.+?)(\.git)?$") {
        $owner = $matches[1]
        $repo = $matches[2]
        Write-Info "Use GitHub to view PRs: https://github.com/$owner/$repo/pulls"
    }
}

function Check-PR {
    param([string]$BranchName)
    
    if (-not $BranchName) {
        Write-Error "Branch name required. Usage: ./admin-tools.ps1 check-pr feature/name"
        exit 1
    }
    
    Write-Header "Checking PR: $BranchName"
    
    # Fetch
    Invoke-GitCommand -Command "fetch" -Arguments $Config.Remote | Out-Null
    
    # Check if exists
    $exists = git ls-remote --heads $Config.Remote $BranchName 2>$null
    if (-not $exists) {
        Write-Error "Branch $BranchName does not exist in $($Config.Remote)"
        exit 1
    }
    
    # Count commits
    $base = git merge-base "$($Config.Remote)/$BranchName" "$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    $commitCount = git rev-list --count "$base..$($Config.Remote)/$BranchName" 2>$null
    
    Write-Info "Commits in branch: $commitCount"
    
    if ($commitCount -eq 1) {
        Write-Success "✓ Only 1 commit (correct squash)"
    }
    else {
        Write-Error "✗ Has $commitCount commits. Must squash to 1."
        Write-Info "Instructions for developer:"
        Write-Host "  ./dev-tools.ps1 squash"
        return
    }
    
    # Check if up-to-date
    $behind = git rev-list --count "$($Config.Remote)/$BranchName..$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    
    if ($behind -eq 0) {
        Write-Success "✓ Branch up-to-date with develop"
    }
    else {
        Write-Error "✗ Branch is $behind commits behind develop"
        Write-Info "Developer must run:"
        Write-Host "  ./dev-tools.ps1 ready"
        return
    }
    
    # Check commit message
    $commitMsg = git log -1 --pretty=%B "$($Config.Remote)/$BranchName" 2>$null
    Write-Host "`nCommit message:" -ForegroundColor $Colors.Info
    Write-Host $commitMsg
    
    if ($commitMsg -match '^(feat|fix|docs|test|refactor|chore|breaking)(\([^)]+\))?:\s.+') {
        Write-Success "✓ Message follows conventional commits"
    }
    else {
        Write-Warning "⚠ Message doesn't follow conventional commits"
    }
    
    if ($commitMsg -match 'Fixes\s+#\d+') {
        Write-Success "✓ Issue reference found"
    }
    else {
        Write-Warning "⚠ No issue reference (Fixes #XXX)"
    }
    
    Write-Host "`n--- Summary ---" -ForegroundColor $Colors.Emphasis
    Write-Success "PR is ready to merge"
    Write-Info "On GitHub: Select 'Squash and merge'"
}

function New-ReleaseCandidate {
    param([string]$Version)
    
    Write-Header "Creating Release Candidate $Version"
    
    # Validate format
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Invalid format. Use: X.Y.Z (e.g.: 1.0.0)"
        exit 1
    }
    
    # Check working directory
    $status = git status --porcelain 2>$null
    if ($status) {
        Write-Error "Working directory not clean"
        exit 1
    }
    
    $releaseBranch = "release/$Version"
    
    # Checkout develop
    Write-Info "Updating $($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.DevelopBranch)")
    if (-not $result) { exit 1 }
    
    # Create release branch
    Write-Info "Creating branch $releaseBranch..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments @("-b", "$releaseBranch")
    if (-not $result) { exit 1 }
    
    # Push
    Write-Info "Pushing to $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "push" -Arguments @("-u", "$($Config.Remote)", "$releaseBranch")
    if (-not $result) { exit 1 }
    
    Write-Success "Release branch $releaseBranch created"
    Write-Info "Now you can:"
    Write-Host "  1. Make bugfixes on this branch (fixes only, no features)"
    Write-Host "  2. Publish RC: git tag v$Version-rc.1 && git push origin v$Version-rc.1"
    Write-Host "  3. When ready: ./admin-tools.ps1 release $Version"
}

function Move-PublicApiToShipped {
    Write-Header "Moving Public API from Unshipped to Shipped"
    
    # Find all projects with PublicAPI files
    $projects = Get-ChildItem -Path "src" -Recurse -Filter "PublicAPI.Unshipped.txt"
    
    $movedCount = 0
    foreach ($unshippedFile in $projects) {
        $projectDir = $unshippedFile.DirectoryName
        $shippedPath = Join-Path $projectDir "PublicAPI.Shipped.txt"
        $unshippedPath = $unshippedFile.FullName
        
        $unshippedContent = Get-Content $unshippedPath -Raw -ErrorAction SilentlyContinue
        
        if ($unshippedContent -and $unshippedContent.Trim()) {
            # Append to Shipped (with newline separator if needed)
            if (Test-Path $shippedPath) {
                # Ensure a separator newline between the existing Shipped content
                # and the appended Unshipped content. Without this, the last line
                # of Shipped fuses with the first line of Unshipped (PublicAPI
                # tooling then sees a single garbled entry).
                $existing = [System.IO.File]::ReadAllText($shippedPath)
                $separator = if ($existing -and -not $existing.EndsWith("`n")) { "`n" } else { "" }
                $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
                [System.IO.File]::AppendAllText($shippedPath, "$separator$unshippedContent", $utf8NoBom)
            }
            else {
                $unshippedContent | Set-Content -Path $shippedPath -Encoding UTF8 -NoNewline
            }

            # Clear Unshipped
            "" | Set-Content -Path $unshippedPath -Encoding UTF8

            Write-Info "Moved API: $($unshippedFile.Directory.Name)"
            $movedCount++
        }
    }
    
    if ($movedCount -gt 0) {
        # Stage the changes
        $apiFiles = Get-ChildItem -Path "src" -Recurse -Filter "PublicAPI.Shipped.txt" -ErrorAction SilentlyContinue
        $apiFiles += Get-ChildItem -Path "src" -Recurse -Filter "PublicAPI.Unshipped.txt" -ErrorAction SilentlyContinue
        foreach ($file in $apiFiles) {
            git add $file.FullName 2>$null
        }
        Write-Success "Moved Public API for $movedCount project(s)"
        return $true
    }
    else {
        Write-Info "No Public API changes to move"
        return $false
    }
}

function Publish-Release {
    param([string]$Version)

    Write-Header "Publishing Release $Version"

    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Invalid format. Use: X.Y.Z"
        exit 1
    }

    $releaseBranch = "release/$Version"
    $tag = "v$Version"

    # ---- STEP 1/8: Pre-flight checks ----
    Write-Info "[STEP 1/8] Pre-flight checks"

    # Working directory must be clean. Publish-Release does multiple checkouts
    # and merges, any of which would fail or silently carry stale state if the
    # tree is dirty. Matches the precondition in New-ReleaseCandidate.
    $status = git status --porcelain 2>$null
    if ($status) {
        Write-Error "Working directory not clean. Commit or stash changes before publishing."
        exit 1
    }

    # Resolve release branch from local OR remote (admin may run on a fresh
    # clone where the RC was created elsewhere and only exists on origin).
    $existsLocal = git branch --list $releaseBranch 2>$null
    $existsRemote = git ls-remote --heads $Config.Remote $releaseBranch 2>$null
    if (-not $existsLocal -and -not $existsRemote) {
        Write-Error "Branch $releaseBranch doesn't exist locally or on $($Config.Remote). Create RC first."
        exit 1
    }
    if (-not $existsLocal -and $existsRemote) {
        Write-Info "Release branch only on $($Config.Remote); fetching..."
        $result = Invoke-GitCommand -Command "fetch" -Arguments @($Config.Remote, $releaseBranch)
        if (-not $result) { exit 1 }
    }

    # Confirmation
    if (-not $Force) {
        Write-Warning "This will:"
        Write-Host "  1. Move Public API Unshipped → Shipped"
        Write-Host "  2. Merge $releaseBranch to $($Config.MainBranch) (squash)"
        Write-Host "  3. Create tag $tag"
        Write-Host "  4. Push to $($Config.Remote)"
        Write-Host "  5. Merge $($Config.MainBranch) into $($Config.DevelopBranch)"
        $confirm = Read-Host "`nContinue? (type 'yes' to confirm)"
        if ($confirm -ne "yes") {
            Write-Info "Cancelled"
            exit 0
        }
    }

    # ---- STEP 2/8: Update release branch and ship Public API ----
    Write-Info "[STEP 2/8] Updating release branch and shipping Public API"
    $result = Invoke-GitCommand -Command "checkout" -Arguments $releaseBranch
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$releaseBranch")
    if (-not $result) { exit 1 }

    $apiMoved = Move-PublicApiToShipped

    if ($apiMoved) {
        $result = Invoke-GitCommand -Command "commit" -Arguments @("-m", "chore: ship public API for release $Version")
        if (-not $result) {
            Write-Warning "Could not commit API changes, continuing..."
        }
        else {
            $result = Invoke-GitCommand -Command "push" -Arguments @("$($Config.Remote)", "$releaseBranch")
            if (-not $result) {
                Write-Warning "Could not push API changes, continuing..."
            }
        }
    }

    # ---- STEP 3/8: Squash-merge release into master ----
    Write-Info "[STEP 3/8] Squash-merging $releaseBranch into $($Config.MainBranch)"
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.MainBranch
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.MainBranch)")
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "merge" -Arguments @("--squash", "$releaseBranch")
    if (-not $result) { exit 1 }

    # ---- STEP 4/8: Commit release and create tag ----
    Write-Info "[STEP 4/8] Committing release and creating tag $tag"
    $result = Invoke-GitCommand -Command "commit" -Arguments @("-m", "Release $Version")
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "tag" -Arguments @("-a", "$tag", "-m", "Release $Version")
    if (-not $result) { exit 1 }

    # ---- STEP 5/8: Push master and tag ----
    Write-Info "[STEP 5/8] Pushing $($Config.MainBranch) and $tag to $($Config.Remote)"
    $result = Invoke-GitCommand -Command "push" -Arguments @("$($Config.Remote)", "$($Config.MainBranch)")
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "push" -Arguments @("$($Config.Remote)", "$tag")
    if (-not $result) { exit 1 }

    # ---- STEP 6/8: Merge master back into develop ----
    Write-Info "[STEP 6/8] Merging $($Config.MainBranch) into $($Config.DevelopBranch)"
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.DevelopBranch)")
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "merge" -Arguments @("$($Config.MainBranch)", "--no-edit")
    if (-not $result) { exit 1 }

    # ---- STEP 7/8: Push develop ----
    Write-Info "[STEP 7/8] Pushing $($Config.DevelopBranch)"
    $result = Invoke-GitCommand -Command "push" -Arguments @("$($Config.Remote)", "$($Config.DevelopBranch)")
    if (-not $result) { exit 1 }

    # ---- STEP 8/8: Cleanup release branch ----
    Write-Info "[STEP 8/8] Deleting $releaseBranch (local + remote)"
    Invoke-GitCommand -Command "branch" -Arguments @("-d", "$releaseBranch") -IgnoreError | Out-Null
    Invoke-GitCommand -Command "push" -Arguments @("$($Config.Remote)", "--delete", "$releaseBranch") -IgnoreError | Out-Null

    Write-Success "Release $Version published"
    Write-Info "CI should publish package to NuGet"
}

function New-Hotfix {
    param([string]$Version)
    
    Write-Header "Creating Hotfix $Version"
    
    if ($Version -notmatch '^\d+\.\d+\.\d+$') {
        Write-Error "Invalid format. Use: X.Y.Z"
        exit 1
    }
    
    $hotfixBranch = "hotfix/$Version"
    
    # Checkout master
    Write-Info "Updating $($Config.MainBranch)..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.MainBranch
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.MainBranch)")
    if (-not $result) { exit 1 }
    
    # Create branch
    Write-Info "Creating branch $hotfixBranch..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments @("-b", "$hotfixBranch")
    if (-not $result) { exit 1 }
    
    # Push
    Write-Info "Pushing to $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "push" -Arguments @("-u", "$($Config.Remote)", "$hotfixBranch")
    if (-not $result) { exit 1 }
    
    Write-Success "Hotfix branch $hotfixBranch created"
    Write-Info "Make the fix, commit, and then: git tag v$Version && git push origin v$Version"
}

function Show-Changelog {
    Write-Header "Pending Changelog"
    
    $lastTag = Get-LastTag
    Write-Info "Last tag: $lastTag"

    $commits = git log "$lastTag..$($Config.Remote)/$($Config.DevelopBranch)" --pretty=format:"%h %s" --no-merges 2>$null
    
    if (-not $commits) {
        Write-Info "No new commits on develop"
        return
    }
    
    Write-Host "`n### Commits since $lastTag`:" -ForegroundColor $Colors.Emphasis
    
    # Group by type
    $types = @{
        'feat' = @()
        'fix' = @()
        'docs' = @()
        'test' = @()
        'refactor' = @()
        'chore' = @()
        'perf' = @()
        'breaking' = @()
        'other' = @()
    }
    
    $commits -split "`n" | ForEach-Object {
        if ($_ -match '^(\w+)(\([^)]+\))?:\s*(.+)$') {
            $type = $matches[1]
            $msg = $matches[3]
            if ($types.ContainsKey($type)) {
                $types[$type] += $msg
            }
            else {
                $types['other'] += $msg
            }
        }
    }
    
    $labels = @{
        'feat' = '✨ Features'
        'fix' = '🐛 Bug Fixes'
        'docs' = '📚 Documentation'
        'test' = '🧪 Tests'
        'refactor' = '♻️ Refactoring'
        'chore' = '🔧 Maintenance'
        'perf' = '⚡ Performance'
        'breaking' = '⚠️ Breaking Changes'
        'other' = '📝 Other'
    }
    
    foreach ($type in $types.Keys) {
        if ($types[$type].Count -gt 0) {
            Write-Host "`n$($labels[$type])" -ForegroundColor $Colors.Emphasis
            $types[$type] | ForEach-Object { Write-Host "  - $_" }
        }
    }
}

function Invoke-Cleanup {
    Write-Header "Cleaning Branches"

    # Checkout develop
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.DevelopBranch)")
    if (-not $result) { exit 1 }

    # Standard merge detection: catches non-squash merges via SHA reachability.
    $standardMerged = git branch --merged $Config.DevelopBranch --format="%(refname:short)" | Where-Object {
        $_ -notin @($Config.MainBranch, $Config.DevelopBranch) -and $_ -notmatch "^\*"
    }

    # Squash-merge detection via GitHub PR state. `git branch --merged` cannot
    # see squash-merged branches because the squash commit on develop has a
    # different SHA than the branch's tip; without this fallback, cleanup
    # never deletes anything in a squash+rebase workflow.
    $allLocal = git branch --format="%(refname:short)" | Where-Object {
        $_ -notin @($Config.MainBranch, $Config.DevelopBranch) -and $_ -notmatch "^\*"
    }
    $prMerged = @()
    foreach ($branch in $allLocal) {
        if ($standardMerged -contains $branch) { continue }
        if (Test-BranchMergedViaPR -Branch $branch) {
            $prMerged += $branch
        }
    }

    if ($standardMerged) {
        Write-Info "Deleting merged branches:"
        foreach ($b in $standardMerged) {
            Write-Host "  - $b"
            Invoke-GitCommand -Command "branch" -Arguments @("-d", "$b") -IgnoreError | Out-Null
        }
    }
    if ($prMerged) {
        Write-Info "Deleting squash-merged branches (verified via gh PR state):"
        foreach ($b in $prMerged) {
            Write-Host "  - $b"
            Invoke-GitCommand -Command "branch" -Arguments @("-D", "$b") -IgnoreError | Out-Null
        }
    }
    if (-not $standardMerged -and -not $prMerged) {
        Write-Info "No merged branches to delete"
    }

    # Prune
    Invoke-GitCommand -Command "remote" -Arguments @("prune", "$($Config.Remote)") -IgnoreError | Out-Null

    Write-Success "Cleanup completed"
}

#endregion

#region Main

if (-not (Test-GitRepository)) {
    Write-Error "Not in a Git repository"
    exit 1
}

switch ($Command) {
    "status" { Show-Status }
    "check-pr" {
        if (-not $Name) {
            Write-Error "Branch name required. Usage: ./admin-tools.ps1 check-pr feature/name"
            exit 1
        }
        Check-PR -BranchName $Name
    }
    "rc" {
        if (-not $Name) {
            Write-Error "Version required. Usage: ./admin-tools.ps1 rc 1.0.0"
            exit 1
        }
        New-ReleaseCandidate -Version $Name
    }
    "release" {
        if (-not $Name) {
            Write-Error "Version required. Usage: ./admin-tools.ps1 release 1.0.0"
            exit 1
        }
        Publish-Release -Version $Name
    }
    "hotfix" {
        if (-not $Name) {
            Write-Error "Version required. Usage: ./admin-tools.ps1 hotfix 1.0.1"
            exit 1
        }
        New-Hotfix -Version $Name
    }
    "changelog" { Show-Changelog }
    "cleanup" { Invoke-Cleanup }
}

Write-Host "`nDone!" -ForegroundColor $Colors.Success

#endregion
