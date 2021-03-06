using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Layout;

namespace TestPanel.Layouts;

public class VirtualizingStackLayout : VirtualizingLayout
{
    private class VirtualizingStackLayoutState
    {
        public int FirstRealizedIndex { get; set; }

        public List<Rect> RealizedRectangles { get; } = new();
        
        public bool IsInfinity { get; set; }
    }

    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<VirtualizingStackLayout, double>(nameof(Spacing));

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<VirtualizingStackLayout, Orientation>(nameof(Orientation), Orientation.Vertical);
        
    public static readonly StyledProperty<double> ItemSizeProperty =
        AvaloniaProperty.Register<VirtualizingStackLayout, double>(nameof(ItemSize), double.NaN);

    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double ItemSize
    {
        get => GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
    }

    protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SpacingProperty 
            || change.Property == OrientationProperty
            || change.Property == ItemSizeProperty)
        {
            InvalidateMeasure();
        }
    }

    protected override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        base.InitializeForContextCore(context);

        if (context.LayoutState is not VirtualizingStackLayoutState)
        {
            context.LayoutState = new VirtualizingStackLayoutState();
        }
    }

    protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        base.UninitializeForContextCore(context);

        context.LayoutState = null;
    }

    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var state = (VirtualizingStackLayoutState)context.LayoutState!;
        var itemSize = ItemSize;
        var isVertical = Orientation == Orientation.Vertical;
        var desiredItemSize = isVertical ? availableSize.Width : availableSize.Height;

        if (double.IsNaN(itemSize))
        {
            var element = context.GetOrCreateElementAt(0);
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            itemSize = isVertical ? element.DesiredSize.Height : element.DesiredSize.Width;
            ItemSize = itemSize;
        }

        var itemOffsetStart = isVertical ? context.RealizationRect.Y : context.RealizationRect.X;
        var itemOffsetEnd = isVertical ? context.RealizationRect.Bottom : context.RealizationRect.Right;
        var firstRealizedIndex = Math.Max((int)(itemOffsetStart / (itemSize + Spacing)) - 1, 0);
        var lastRealizedIndex = Math.Min((int)(itemOffsetEnd / (itemSize + Spacing)) + 1, context.ItemCount);

        state.IsInfinity = double.IsInfinity(desiredItemSize);

        var finalDesiredItemSize = 0.0;

        state.RealizedRectangles.Clear();
        state.FirstRealizedIndex = firstRealizedIndex;

        for (var index = firstRealizedIndex; index < lastRealizedIndex; index++)
        {
            var width = isVertical ? desiredItemSize : itemSize;
            var height = isVertical ? itemSize : desiredItemSize;

            var element = context.GetOrCreateElementAt(index);
            element.Measure(new Size(width, height));

            var offset = index * (itemSize + Spacing);
            var rect = new Rect(
                isVertical ? 0.0 : offset,
                isVertical ? offset : 0.0, 
                width, 
                height);
            state.RealizedRectangles.Add(rect);

            if (state.IsInfinity)
            {
                finalDesiredItemSize = Math.Max(0, isVertical ? element.DesiredSize.Width : element.DesiredSize.Height);
            }
        }

        var extentSize = (context.ItemCount - 1) * (itemSize + Spacing) + itemSize;

        if (state.IsInfinity)
        {
            desiredItemSize = finalDesiredItemSize;
        }

        return new Size(
            isVertical ? desiredItemSize : extentSize, 
            isVertical ? extentSize : desiredItemSize);
    }

    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var state = (VirtualizingStackLayoutState)context.LayoutState!;
        var currentIndex = state.FirstRealizedIndex;
        var isVertical = Orientation == Orientation.Vertical;
        var desiredItemSize = isVertical ? finalSize.Width : finalSize.Height;

        foreach (var rect in state.RealizedRectangles)
        {
            var element = context.GetOrCreateElementAt(currentIndex);
            element.Arrange(state.IsInfinity 
                ? isVertical ? rect.WithWidth(desiredItemSize) : rect.WithHeight(desiredItemSize)
                : rect);
            currentIndex++;
        }

        return finalSize;
    }
}
