namespace BlazOrbit.Components;

internal interface ICarouselJsCallback
{
    Task OnSwipeLeftAsync();
    Task OnSwipeRightAsync();
}
