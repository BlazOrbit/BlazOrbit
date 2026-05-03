#!/usr/bin/env pwsh
#requires -Version 7.0
<#
.SYNOPSIS
    Development tools for BlazOrbit contributors
    STRATEGY: Each PR = 1 commit (squash) + rebase before merging

.DESCRIPTION
    Script for contributor developers.
    Facilitates the flow: feature → squash to 1 commit → rebase → PR

.EXAMPLE
    ./dev-tools.ps1 feat css-tokens
    Creates a new feature branch from develop

.EXAMPLE
    ./dev-tools.ps1 commit "feat(css): add scrollbar tokens"
    Commit with convention + issue reference

.EXAMPLE
    ./dev-tools.ps1 squash
    Squashes all feature commits into 1

.EXAMPLE
    ./dev-tools.ps1 ready
    Prepares for PR: rebase + 1 commit verification

.EXAMPLE
    ./dev-tools.ps1 fix-conflict
    After resolving conflicts, squash fixups

.NOTES
    Author: Samuel Maícas (@cdcsharp)
    Version: 2.0.0 - Squash+Rebase Strategy
#>
<#
Conventional Commit actions: feat, fix, refactor, perf, style, test, docs, build, ops, chore
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("feat", "fix", "refactor", "perf", "style", "test", "docs", "build", "ops", "chore", "sync", "commit", "squash", "ready", "fix-conflict", "push", "pr", "cleanup", "status")]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Name,

    [Parameter(Position = 2)]
    [string]$Description,

    [Parameter()]
    [switch]$Force
)

# Configuration
$Config = @{
    DevelopBranch = "develop"
    MainBranch = "master"
    Remote = "origin"
    FeaturePrefix = "feat"
    FixPrefix = "fix"
    RefactorPrefix = "refactor"
    PerfPrefix = "perf"
    StylePrefix = "style"
    TestPrefix = "test"
    DocsPrefix = "docs"
    BuildPrefix = "build"
    OpsPrefix = "ops"
    ChorePrefix = "chore"
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

function Test-WorkingDirectoryClean {
    # NOTE: counts untracked files as "dirty" (git status --porcelain default).
    # If you stash to make this return true again, use `git stash push -u`
    # (or Save-Stash) so untracked files are also captured. Plain `git stash push`
    # leaves untracked in place, which makes this check fail again on the next
    # call and can cascade into double-stash bugs.
    $status = git status --porcelain 2>$null
    return [string]::IsNullOrWhiteSpace($status)
}

function Get-CurrentBranch {
    return git branch --show-current 2>$null
}

function Test-BranchExists {
    param([string]$Branch)
    $result = git branch --list $Branch 2>$null
    return -not [string]::IsNullOrWhiteSpace($result)
}

function Get-CommitCount {
    $base = git merge-base HEAD "$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    if (-not $base) { return 0 }
    $count = git rev-list --count "$base..HEAD" 2>$null
    return [int]$count
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

function Save-Stash {
    <#
    .SYNOPSIS
    Stash current changes (including untracked files via -u) under a unique
    tag, returning that tag for later use with Restore-Stash. Returns $null
    when there is nothing to stash. Exits the script on actual stash failure.
    #>
    param([string]$ContextLabel = "auto")

    $timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $rand = [Guid]::NewGuid().ToString('N').Substring(0, 8)
    $tag = "dev-tools-$ContextLabel-$timestamp-$rand"

    $output = & git stash push -u -m $tag 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "git stash push failed: $output"
        exit 1
    }

    # `git stash push` exits 0 with "No local changes to save" when there is
    # nothing to stash. Verify an entry actually exists before claiming the
    # caller has something to restore later.
    $entries = git stash list 2>$null
    $found = $entries | Where-Object { $_ -like "*$tag*" }
    if (-not $found) {
        return $null
    }
    return $tag
}

function Restore-Stash {
    <#
    .SYNOPSIS
    Pop the stash entry matching a tag returned by Save-Stash. Locates the
    entry by message rather than stack position, so unrelated stashes left
    over from prior failed runs do not get popped by accident. Returns $true
    on success (or no-op when $Tag is $null), $false on failure.
    #>
    param([string]$Tag)

    if (-not $Tag) {
        return $true
    }

    $entries = git stash list 2>$null
    $line = $entries | Where-Object { $_ -like "*$Tag*" } | Select-Object -First 1
    if (-not $line) {
        Write-Warning "Stash entry '$Tag' not found (already popped or dropped)."
        return $false
    }

    if ($line -notmatch '^(stash@\{\d+\})') {
        Write-Warning "Could not parse stash ref from line: $line"
        return $false
    }
    $ref = $matches[1]

    $output = & git stash pop $ref 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "git stash pop $ref failed: $output"
        Write-Info "Your changes are still in the stash. Recover with:"
        Write-Host "  git stash list   # find the entry tagged '$Tag'"
        Write-Host "  git stash pop $ref"
        return $false
    }
    return $true
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

function Show-Status {
    Write-Header "Repository Status"
    
    $currentBranch = Get-CurrentBranch
    Write-Info "Current branch: $currentBranch"
    
    # Count commits in this branch vs develop
    $commitCount = Get-CommitCount
    if ($commitCount -gt 0) {
        if ($commitCount -eq 1) {
            Write-Success "✓ Branch ready: 1 commit (squash done)"
        }
        else {
            Write-Warning "⚠ Branch has $commitCount commits. Run: ./dev-tools.ps1 squash"
        }
    }
    
    # Working directory status
    $clean = Test-WorkingDirectoryClean
    if ($clean) {
        Write-Info "Working directory clean"
    }
    else {
        Write-Warning "Uncommitted changes:"
        git status --short
    }
    
    # Check if up-to-date with develop
    $behind = git rev-list --count "HEAD..$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    if ($behind -and [int]$behind -gt 0) {
        Write-Warning "$behind new commits on develop. Run: ./dev-tools.ps1 ready"
    }
}

function Sync-Develop {
    param(
        # When set, the caller has already stashed; skip our own stash/pop to
        # avoid the double-stash cascade that misorders the stash stack and
        # makes a later pop pick up the wrong entry from prior runs.
        [switch]$NoStash
    )

    Write-Header "Syncing $($Config.DevelopBranch)"

    # Stash if there are changes (unless caller already handled it)
    $stashTag = $null
    if (-not $NoStash -and -not (Test-WorkingDirectoryClean)) {
        Write-Warning "Uncommitted changes. Stashing..."
        $stashTag = Save-Stash -ContextLabel "sync"
    }

    # Checkout develop
    $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
    if (-not $result) { exit 1 }

    # Pull
    Write-Info "Pull from $($Config.Remote)/$($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.DevelopBranch)")
    if (-not $result) { exit 1 }

    # Restore stash
    if ($stashTag) {
        Write-Info "Restoring changes..."
        if (-not (Restore-Stash -Tag $stashTag)) {
            exit 1
        }
    }

    Write-Success "$($Config.DevelopBranch) updated"
}

function New-FeatureBranch {
    param(
        [string]$BranchName,
        [string]$Prefix
    )

    Write-Header "Creating branch $Prefix/$BranchName"

    # Validate name
    if ($BranchName -match '[\s\\:*?"<>|]') {
        Write-Error "Invalid branch name. No spaces or special characters."
        exit 1
    }

    # Stash all working tree changes (tracked + untracked, via Save-Stash) once.
    # We then pass -NoStash to Sync-Develop so it does not stash again on top —
    # the prior cascade left an outer entry that pop later picked up the wrong
    # slot from when older stashes existed.
    $stashTag = $null
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Warning "Uncommitted changes. Will auto-stash."
        $stashTag = Save-Stash -ContextLabel "branch"
    }

    # Sync develop first (we already stashed)
    Sync-Develop -NoStash

    # Create branch
    $fullBranchName = "$Prefix/$BranchName"
    Write-Info "Creating branch $fullBranchName..."
    $result = Invoke-GitCommand -Command "checkout" -Arguments @("-b", "$fullBranchName")
    if (-not $result) { exit 1 }

    # Push with tracking
    Write-Info "Pushing to $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "push" -Arguments @("-u", "$($Config.Remote)", "$fullBranchName")
    if (-not $result) { exit 1 }

    # Restore stash onto the new branch
    if ($stashTag) {
        Write-Info "Restoring stashed changes..."
        if (-not (Restore-Stash -Tag $stashTag)) {
            exit 1
        }
    }

    Write-Success "Branch $fullBranchName created and pushed"
    Write-Info "Work on your changes and commit frequently."
    Write-Info "When done: ./dev-tools.ps1 squash"
}

function New-Commit {
    param([string]$Message)
    
    if (-not $Message) {
        Write-Error "Message required. Usage: ./dev-tools.ps1 commit \"type(scope): description\""
        Write-Info "Format: <type>(<scope>): <description>"
        Write-Info "Types: feat, fix, docs, test, refactor, chore, breaking"
        Write-Info "Example: feat(css): add scrollbar tokens to FeatureDefinitions"
        exit 1
    }
    
    # Check conventional commit format
    if ($Message -notmatch '^(feat|fix|docs|test|refactor|chore|perf|breaking)(\([^)]+\))?:\s.+') {
        Write-Warning "Message doesn't follow conventional commits"
        Write-Info "Expected format: type(scope): description"
    }
    
    # Add issue if provided
    if ($Description -match '#\d+') {
        $Message = "$Message`n`nFixes $Description"
    }

    # Stage everything: tracked changes (modified/deleted) AND untracked (new
    # files). `git commit -am` only auto-stages tracked changes, so brand-new
    # files were silently skipped from commits before. `git add -A` covers
    # add/modify/delete uniformly so the commit reflects the full working tree.
    $result = Invoke-GitCommand -Command "add" -Arguments @("-A")
    if (-not $result) { exit 1 }

    $result = Invoke-GitCommand -Command "commit" -Arguments @("-m", "$Message")
    if (-not $result) { exit 1 }

    Write-Success "Commit created"
    
    # Show count
    $count = Get-CommitCount
    Write-Info "Commits in this branch: $count"
    if ($count -gt 1) {
        Write-Info "Remember: before PR run ./dev-tools.ps1 squash"
    }
}

function Invoke-Squash {
    Write-Header "Squashing commits into 1"
    
    $commitCount = Get-CommitCount
    
    if ($commitCount -le 1) {
        Write-Success "Already only 1 commit. No squash needed."
        return
    }
    
    Write-Info "Commits to combine: $commitCount"
    Write-Info "Recent commits:"
    git log --oneline -$commitCount
    
    # Ask for final commit message
    Write-Host "`nEnter message for final commit (type(scope): description):" -ForegroundColor $Colors.Emphasis
    $defaultMsg = git log -1 --pretty=%B
    $finalMessage = Read-Host "Message [$defaultMsg]"
    
    if (-not $finalMessage) {
        $finalMessage = $defaultMsg
    }
    
    # Squash
    Write-Info "Squashing..."
    $base = git merge-base HEAD "$($Config.Remote)/$($Config.DevelopBranch)"
    
    $result = Invoke-GitCommand -Command "reset" -Arguments @("--soft", "$base")
    if (-not $result) { exit 1 }
    
    $result = Invoke-GitCommand -Command "commit" -Arguments @("-m", "$finalMessage")
    if (-not $result) { exit 1 }
    
    Write-Success "Squash completed. Now 1 commit."
    Write-Info "Run ./dev-tools.ps1 ready to prepare PR"
}

function Ready-ForPR {
    Write-Header "Preparing for PR (Squash + Rebase)"
    
    $currentBranch = Get-CurrentBranch
    
    if ($currentBranch -eq $Config.DevelopBranch) {
        Write-Error "You're on $($Config.DevelopBranch). Create a feature branch first."
        exit 1
    }
    
    # Check for changes
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Error "Uncommitted changes. Commit or stash first."
        exit 1
    }
    
    # Step 1: Check squash
    $commitCount = Get-CommitCount
    if ($commitCount -gt 1) {
        Write-Warning "Has $commitCount commits. Auto-squashing..."
        Invoke-Squash
    }
    
    # Step 2: Fetch
    Write-Info "Fetching $($Config.Remote)..."
    $result = Invoke-GitCommand -Command "fetch" -Arguments $Config.Remote
    if (-not $result) { exit 1 }
    
    # Step 3: Rebase
    Write-Info "Rebasing onto $($Config.Remote)/$($Config.DevelopBranch)..."
    $result = Invoke-GitCommand -Command "rebase" -Arguments @("$($Config.Remote)/$($Config.DevelopBranch)")
    
    if (-not $result) {
        Write-Error "`n❌ REBASE FAILED - CONFLICTS"
        Write-Info "Resolve conflicts manually:"
        Write-Host "  1. Edit conflicted files (look for <<<<<<<)"
        Write-Host "  2. git add <files>"
        Write-Host "  3. git rebase --continue"
        Write-Host "  4. ./dev-tools.ps1 fix-conflict"
        Write-Host "`nTo abort: git rebase --abort"
        exit 1
    }
    
    # Step 4: Verify still 1 commit
    $commitCount = Get-CommitCount
    if ($commitCount -gt 1) {
        Write-Warning "Rebase created $commitCount commits. Squashing..."
        Invoke-Squash
    }
    
    # Step 5: Push force-with-lease
    Write-Info "Pushing with force-with-lease..."
    $result = Invoke-GitCommand -Command "push" -Arguments @("-u", "$($Config.Remote)", "$currentBranch", "--force-with-lease")
    if (-not $result) { exit 1 }
    
    Write-Success "Branch ready for PR!"
    Write-Info "Verification:"
    Write-Host "  ✓ 1 commit (squash)"
    Write-Host "  ✓ Rebased onto current develop"
    Write-Host "  ✓ No conflicts"
    Write-Host "`nCreate PR on GitHub or run: ./dev-tools.ps1 pr"
}

function Fix-Conflict {
    Write-Header "Finishing conflict resolution"
    
    # Check for pending conflicts
    $status = git status --porcelain 2>$null
    if ($status -match "^(UU|AA|DD|AU|UA|DU|UD)") {
        Write-Error "Still unresolved conflicts. Edit files and git add."
        git status --short
        exit 1
    }
    
    # Check if in middle of rebase
    $rebaseDir = git rev-parse --git-path rebase-merge 2>$null
    $rebaseApplyDir = git rev-parse --git-path rebase-apply 2>$null
    
    if ((Test-Path $rebaseDir) -or (Test-Path $rebaseApplyDir)) {
        Write-Info "Continuing rebase..."
        $result = Invoke-GitCommand -Command "rebase" -Arguments "--continue"
        if (-not $result) {
            Write-Error "Rebase still failing. Check conflicts."
            exit 1
        }
    }
    
    # Check how many commits we have now
    $commitCount = Get-CommitCount
    Write-Info "Commits after resolving conflicts: $commitCount"
    
    if ($commitCount -gt 1) {
        Write-Warning "Has $commitCount commits (including conflict fixups)"
        Write-Info "Final squash..."
        Invoke-Squash
    }
    
    # Push
    $currentBranch = Get-CurrentBranch
    Write-Info "Pushing with force-with-lease..."
    $result = Invoke-GitCommand -Command "push" -Arguments @("-u", "$($Config.Remote)", "$currentBranch", "--force-with-lease")
    if (-not $result) { exit 1 }
    
    Write-Success "Conflicts resolved and code updated"
    Write-Info "PR should be mergeable now"
}

function Push-Changes {
    Write-Header "Pushing changes"
    
    $currentBranch = Get-CurrentBranch
    
    # Check for changes
    if (-not (Test-WorkingDirectoryClean)) {
        Write-Error "Uncommitted changes. Commit first."
        exit 1
    }
    
    # Check squash if feature branch
    if ($currentBranch -ne $Config.DevelopBranch) {
        $commitCount = Get-CommitCount
        if ($commitCount -gt 1) {
            Write-Warning "⚠️  You have $commitCount commits. Remember to squash before PR."
        }
    }
    
    # Push
    Write-Info "Pushing..."
    $result = Invoke-GitCommand -Command "push" -Arguments @("-u", "$($Config.Remote)", "$currentBranch", "--force-with-lease")
    if (-not $result) { exit 1 }
    
    Write-Success "Push completed"
}

function New-PullRequest {
    Write-Header "Creating Pull Request"
    
    $currentBranch = Get-CurrentBranch
    
    if ($currentBranch -eq $Config.DevelopBranch) {
        Write-Error "Cannot create PR from $($Config.DevelopBranch)"
        exit 1
    }
    
    # Verifications
    $commitCount = Get-CommitCount
    if ($commitCount -gt 1) {
        Write-Error "Has $commitCount commits. Must squash first: ./dev-tools.ps1 squash"
        exit 1
    }
    
    $behind = git rev-list --count "HEAD..$($Config.Remote)/$($Config.DevelopBranch)" 2>$null
    if ($behind -gt 0) {
        Write-Error "$behind commits behind develop. Update first: ./dev-tools.ps1 ready"
        exit 1
    }
    
    # Open PR creation URL
    $repoUrl = git remote get-url $Config.Remote 2>$null
    if ($repoUrl -match "github.com[:/](.+)/(.+?)(\.git)?$") {
        $owner = $matches[1]
        $repo = $matches[2]
        
        $prUrl = "https://github.com/$owner/$repo/compare/$($Config.DevelopBranch)...$currentBranch?expand=1"
        
        Write-Info "Opening browser to create PR for branch '$currentBranch'..."
        Start-Process $prUrl
    }
    else {
        Write-Warning "Could not detect repo URL. Create PR manually."
    }
    
    Write-Success "PR ready to create"
}

function Invoke-Cleanup {
    Write-Header "Cleaning Local Branches"

    $currentBranch = Get-CurrentBranch

    # Go back to develop
    if ($currentBranch -ne $Config.DevelopBranch) {
        $result = Invoke-GitCommand -Command "checkout" -Arguments $Config.DevelopBranch
        if (-not $result) { exit 1 }
    }

    # Pull
    $result = Invoke-GitCommand -Command "pull" -Arguments @("$($Config.Remote)", "$($Config.DevelopBranch)")
    if (-not $result) { exit 1 }

    # Standard merge detection: catches non-squash merges via SHA reachability.
    $standardMerged = git branch --merged $Config.DevelopBranch --format="%(refname:short)" | Where-Object {
        $_ -notin @($Config.MainBranch, $Config.DevelopBranch) -and
        $_ -notmatch "^\*"
    }

    # Squash-merge detection via GitHub PR state. `git branch --merged` cannot
    # see squash-merged branches because the squash commit on develop has a
    # different SHA than the branch's tip; without this fallback, cleanup
    # never deletes anything in a squash+rebase workflow.
    $allLocal = git branch --format="%(refname:short)" | Where-Object {
        $_ -notin @($Config.MainBranch, $Config.DevelopBranch) -and
        $_ -notmatch "^\*"
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

# Verify we're in a git repo
if (-not (Test-GitRepository)) {
    Write-Error "Not in a Git repository"
    exit 1
}

# Execute command
switch ($Command) {
    "status" { Show-Status }
    "sync" { Sync-Develop }
    "feat" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 feat feature-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.FeaturePrefix
    }
    "fix" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 fix fix-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.FixPrefix
    }
    "refactor" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 refactor refactor-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.RefactorPrefix
    }
    "perf" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 perf perf-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.PerfPrefix
    }
    "style" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 style style-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.StylePrefix
    }
    "test" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 test test-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.TestPrefix
    }
    "docs" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 docs docs-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.DocsPrefix
    }
    "build" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 build build-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.BuildPrefix
    }
    "ops" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 ops ops-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.OpsPrefix
    }
    "chore" {
        if (-not $Name) {
            Write-Error "Branch Name required. Usage: ./dev-tools.ps1 chore chore-name"
            exit 1
        }
        New-FeatureBranch -BranchName $Name -Prefix $Config.ChorePrefix
    }
    "commit" { New-Commit -Message $Name }
    "squash" { Invoke-Squash }
    "ready" { Ready-ForPR }
    "fix-conflict" { Fix-Conflict }
    "push" { Push-Changes }
    "pr" { New-PullRequest }
    "cleanup" { Invoke-Cleanup }
}

Write-Host "`nDone!" -ForegroundColor $Colors.Success

#endregion
