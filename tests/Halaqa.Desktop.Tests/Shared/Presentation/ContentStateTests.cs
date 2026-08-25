using Xunit;
using Halaqa.Desktop.Shared.Presentation.State;

namespace Halaqa.Desktop.Tests.Shared.Presentation;

public sealed class ContentStateTests
{
    [Fact]
    public void LoadingState_IsBlockingAndPreservesOptionalMessage()
    {
        var state = ContentState.Loading("جار التحميل");

        Assert.Equal(ContentStateKind.Loading, state.Kind);
        Assert.True(state.IsBlocking);
        Assert.Equal("جار التحميل", state.Message);
    }

    [Fact]
    public void EmptyState_IsNotBlockingAndRequiresItsMessage()
    {
        var state = ContentState.Empty("لا توجد حلقات متاحة حالياً.");

        Assert.Equal(ContentStateKind.Empty, state.Kind);
        Assert.False(state.IsBlocking);
        Assert.Equal("لا توجد حلقات متاحة حالياً.", state.Message);
    }

    [Fact]
    public void ErrorState_IsBlocking()
    {
        var state = ContentState.Error("تعذر تحميل البيانات.");

        Assert.Equal(ContentStateKind.Error, state.Kind);
        Assert.True(state.IsBlocking);
    }
}
