// SnapshotTests/RazorSnapshotTests.cs
using BlazOrbit.SyntaxHighlight.Languages;
using BlazOrbit.SyntaxHighlight.Tokens;

namespace BlazOrbit.SyntaxHighlight.Tests.SnapshotTests;

public class RazorSnapshotTests
{
    [Fact]
    public Task Tokenize_CompleteBlazorComponent_MatchesSnapshot()
    {
        string code = """
            @page "/customers/{CustomerId:int}"
            @using Microsoft.AspNetCore.Authorization
            @using MyApp.Services
            @inject ICustomerService CustomerService
            @inject NavigationManager Navigation
            @attribute [Authorize(Roles = "Admin,Manager")]
            @rendermode InteractiveServer

            <PageTitle>Customer Details - @customer?.Name</PageTitle>

            @if (isLoading)
            {
                <div class="spinner-container">
                    <LoadingSpinner Size="SpinnerSize.Large" />
                </div>
            }
            else if (customer is null)
            {
                <Alert Type="AlertType.Warning">
                    <AlertTitle>Not Found</AlertTitle>
                    <AlertContent>Customer with ID @CustomerId was not found.</AlertContent>
                </Alert>
            }
            else
            {
                <div class="customer-details">
                    <header>
                        <h1>@customer.Name</h1>
                        <Badge Color="@GetStatusColor(customer.Status)">@customer.Status</Badge>
                    </header>

                    <EditForm Model="@editModel" OnValidSubmit="@HandleSubmit">
                        <DataAnnotationsValidator />
                        <ValidationSummary />

                        <div class="form-group">
                            <label for="name">Name</label>
                            <InputText id="name" @bind-Value="editModel.Name" class="form-control" />
                            <ValidationMessage For="@(() => editModel.Name)" />
                        </div>

                        <div class="form-group">
                            <label for="email">Email</label>
                            <InputText id="email" @bind-Value="editModel.Email" type="email" />
                        </div>

                        <div class="form-group">
                            <label for="tier">Tier</label>
                            <InputSelect @bind-Value="editModel.Tier" TValue="CustomerTier">
                                @foreach (var tier in Enum.GetValues<CustomerTier>())
                                {
                                    <option value="@tier">@tier.GetDisplayName()</option>
                                }
                            </InputSelect>
                        </div>

                        <div class="button-group">
                            <Button Type="ButtonType.Submit" Variant="Primary" Disabled="@isSaving">
                                @if (isSaving)
                                {
                                    <span class="spinner-border spinner-border-sm"></span>
                                    <span>Saving...</span>
                                }
                                else
                                {
                                    <span>Save Changes</span>
                                }
                            </Button>
                            <Button Type="ButtonType.Button" Variant="Secondary" OnClick="@Cancel">
                                Cancel
                            </Button>
                        </div>
                    </EditForm>

                    <section class="orders-section">
                        <h2>Recent Orders</h2>
                        <DataGrid TItem="Order" Items="@customer.Orders" Pageable="true" PageSize="10">
                            <Columns>
                                <Column TItem="Order" Field="@(o => o.Id)" Title="Order #" />
                                <Column TItem="Order" Field="@(o => o.Date)" Title="Date" Format="d" />
                                <Column TItem="Order" Field="@(o => o.Total)" Title="Total" Format="C" />
                                <Column TItem="Order" Title="Actions">
                                    <Template Context="order">
                                        <a href="/orders/@order.Id">View</a>
                                    </Template>
                                </Column>
                            </Columns>
                        </DataGrid>
                    </section>
                </div>
            }

            @* Modal for confirmation *@
            <Modal @bind-IsVisible="showConfirmModal" Title="Confirm Changes">
                <BodyContent>
                    <p>Are you sure you want to save these changes?</p>
                </BodyContent>
                <FooterContent>
                    <Button OnClick="@(() => showConfirmModal = false)">Cancel</Button>
                    <Button Variant="Primary" OnClick="@ConfirmSave">Confirm</Button>
                </FooterContent>
            </Modal>

            @code {
                [Parameter]
                public int CustomerId { get; set; }

                [CascadingParameter]
                public Task<AuthenticationState>? AuthState { get; set; }

                private Customer? customer;
                private CustomerEditModel editModel = new();
                private bool isLoading = true;
                private bool isSaving;
                private bool showConfirmModal;

                protected override async Task OnInitializedAsync()
                {
                    try
                    {
                        customer = await CustomerService.GetByIdAsync(CustomerId);
                        if (customer is not null)
                        {
                            editModel = new CustomerEditModel
                            {
                                Name = customer.Name,
                                Email = customer.Email,
                                Tier = customer.Tier
                            };
                        }
                    }
                    finally
                    {
                        isLoading = false;
                    }
                }

                private async Task HandleSubmit()
                {
                    showConfirmModal = true;
                }

                private async Task ConfirmSave()
                {
                    isSaving = true;
                    showConfirmModal = false;

                    try
                    {
                        await CustomerService.UpdateAsync(CustomerId, editModel);
                        Navigation.NavigateTo("/customers", forceLoad: false);
                    }
                    catch (Exception ex)
                    {
                        // Handle error
                    }
                    finally
                    {
                        isSaving = false;
                    }
                }

                private void Cancel() => Navigation.NavigateTo("/customers");

                private static Color GetStatusColor(CustomerStatus status) => status switch
                {
                    CustomerStatus.Active => Color.Success,
                    CustomerStatus.Inactive => Color.Warning,
                    CustomerStatus.Suspended => Color.Danger,
                    _ => Color.Secondary
                };
            }
            """;

        IReadOnlyList<Token> tokens = RazorLanguage.Instance.Tokenize(code);

        return Verify(tokens.Select(t => new { t.Type, t.Value }));
    }
}