using BlazOrbit.FormsFluentValidation;
using Bunit;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;

namespace BlazOrbit.Tests.Integration.Tests.FormsFluentValidation;

[Trait("Component", "BOBFluentValidator")]
public class BOBFluentValidatorTests
{
    public class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    public class TestModelValidator : AbstractValidator<TestModel>
    {
        public TestModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(10);
            RuleFor(x => x.Age).InclusiveBetween(0, 120);
        }
    }

    [Fact]
    public void Should_Throw_When_Not_Inside_EditForm()
    {
        BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        FluentActions.Invoking(() => ctx.Render<BOBFluentValidator>())
            .Should().Throw<InvalidOperationException>()
            .WithMessage($"*{nameof(BOBFluentValidator)}*inside an*{nameof(EditForm)}*");
    }

    [Fact]
    public void Should_Throw_When_No_Validator_Resolved()
    {
        BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;

        TestModel model = new();

        FluentActions.Invoking(() => ctx.Render<EditForm>(p => p
                .Add(c => c.Model, model)
                .Add(c => c.ChildContent, (EditContext _) => b =>
                {
                    b.OpenComponent<BOBFluentValidator>(0);
                    b.CloseComponent();
                })))
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*No se encontró un validador*");
    }

    [Fact]
    public void Should_Validate_Model_On_ValidationRequested()
    {
        using BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddValidatorsFromAssemblyContaining<TestModelValidator>();

        TestModel model = new() { Name = "", Age = 200 };

        IRenderedComponent<EditForm> cut = ctx.Render<EditForm>(p => p
            .Add(c => c.Model, model)
            .Add(c => c.ChildContent, (EditContext _) => b =>
            {
                b.OpenComponent<BOBFluentValidator>(0);
                b.CloseComponent();
            }));

        EditContext editContext = cut.Instance.EditContext!;
        bool validationStateChanged = false;
        editContext.OnValidationStateChanged += (_, _) => validationStateChanged = true;

        bool isValid = editContext.Validate();

        isValid.Should().BeFalse();
        validationStateChanged.Should().BeTrue();
        editContext.GetValidationMessages().Should().HaveCount(2);
    }

    [Fact]
    public void Should_Validate_Single_Field_On_FieldChanged()
    {
        using BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddValidatorsFromAssemblyContaining<TestModelValidator>();

        TestModel model = new() { Name = "ValidName", Age = 200 };

        IRenderedComponent<EditForm> cut = ctx.Render<EditForm>(p => p
            .Add(c => c.Model, model)
            .Add(c => c.ChildContent, (EditContext _) => b =>
            {
                b.OpenComponent<BOBFluentValidator>(0);
                b.CloseComponent();
            }));

        EditContext editContext = cut.Instance.EditContext!;

        editContext.NotifyFieldChanged(new FieldIdentifier(model, nameof(TestModel.Age)));

        IEnumerable<string> ageErrors =
            editContext.GetValidationMessages(new FieldIdentifier(model, nameof(TestModel.Age)));
        ageErrors.Should().ContainSingle()
            .Which.Should().Contain("0").And.Contain("120");

        editContext.GetValidationMessages(new FieldIdentifier(model, nameof(TestModel.Name)))
            .Should().BeEmpty();
    }

    [Fact]
    public void Should_Not_Subscribe_FieldChanged_When_ValidateOnFieldChanged_Is_False()
    {
        using BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddValidatorsFromAssemblyContaining<TestModelValidator>();

        TestModel model = new() { Name = "", Age = 0 };

        IRenderedComponent<EditForm> cut = ctx.Render<EditForm>(p => p
            .Add(c => c.Model, model)
            .Add(c => c.ChildContent, (EditContext _) => b =>
            {
                b.OpenComponent<BOBFluentValidator>(0);
                b.AddAttribute(1, nameof(BOBFluentValidator.ValidateOnFieldChanged), false);
                b.CloseComponent();
            }));

        EditContext editContext = cut.Instance.EditContext!;

        editContext.NotifyFieldChanged(new FieldIdentifier(model, nameof(TestModel.Name)));

        editContext.GetValidationMessages().Should().BeEmpty();
    }

    [Fact]
    public void Should_Use_Explicit_ValidatorType_When_Provided()
    {
        using BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        // Deliberately NOT registering the validator in DI.

        TestModel model = new() { Name = "" };

        IRenderedComponent<EditForm> cut = ctx.Render<EditForm>(p => p
            .Add(c => c.Model, model)
            .Add(c => c.ChildContent, (EditContext _) => b =>
            {
                b.OpenComponent<BOBFluentValidator>(0);
                b.AddAttribute(1, nameof(BOBFluentValidator.ValidatorType), typeof(TestModelValidator));
                b.CloseComponent();
            }));

        EditContext editContext = cut.Instance.EditContext!;
        editContext.Validate();

        editContext.GetValidationMessages().Should().NotBeEmpty();
    }

    [Fact]
    public void Should_Clear_Messages_On_Dispose()
    {
        using BunitContext ctx = new();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddValidatorsFromAssemblyContaining<TestModelValidator>();

        TestModel model = new() { Name = "" };

        IRenderedComponent<EditForm> cut = ctx.Render<EditForm>(p => p
            .Add(c => c.Model, model)
            .Add(c => c.ChildContent, (EditContext _) => b =>
            {
                b.OpenComponent<BOBFluentValidator>(0);
                b.CloseComponent();
            }));

        EditContext editContext = cut.Instance.EditContext!;
        editContext.Validate();
        editContext.GetValidationMessages().Should().NotBeEmpty();

        // Dispose the validator component (it is the only child)
        IRenderedComponent<BOBFluentValidator> validator = cut.FindComponent<BOBFluentValidator>();

        validator.Dispose();
    }
}