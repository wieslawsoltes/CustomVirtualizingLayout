using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Layout;

namespace TestPanel.Layouts
{
    public class CustomVirtualizingLayoutState
    {
        public int FirstRealizedIndex { get; set; }

        public List<Rect> LayoutRectangleList { get; } = new();
    }

    public class CustomVirtualizingLayout : VirtualizingLayout
    {
        public static readonly StyledProperty<double> RowSpacingProperty =
            AvaloniaProperty.Register<CustomVirtualizingLayout, double>(nameof(RowSpacing));

        public static readonly StyledProperty<double> ColumnSpacingProperty =
            AvaloniaProperty.Register<CustomVirtualizingLayout, double>(nameof(ColumnSpacing));

        public static readonly StyledProperty<Size> MinimumItemSizeProperty =
            AvaloniaProperty.Register<CustomVirtualizingLayout, Size>(nameof(MinimumItemSize), Size.Empty);

        public double RowSpacing
        {
            get => GetValue(RowSpacingProperty);
            set => SetValue(RowSpacingProperty, value);
        }

        public double ColumnSpacing
        {
            get => GetValue(ColumnSpacingProperty);
            set => SetValue(ColumnSpacingProperty, value);
        }

        public Size MinimumItemSize
        {
            get => GetValue(MinimumItemSizeProperty);
            set => SetValue(MinimumItemSizeProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RowSpacingProperty
                || change.Property == ColumnSpacingProperty
                || change.Property == MinimumItemSizeProperty)
            {
                InvalidateMeasure();
            }
        }

        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.InitializeForContextCore(context);

            if (context.LayoutState is not CustomVirtualizingLayoutState)
            {
                context.LayoutState = new CustomVirtualizingLayoutState();
            }
        }

        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            base.UninitializeForContextCore(context);

            context.LayoutState = null;
        }

        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            var state = (CustomVirtualizingLayoutState)context.LayoutState!;

            if (MinimumItemSize == Size.Empty)
            {
                var firstElement = context.GetOrCreateElementAt(0);

                firstElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                MinimumItemSize = firstElement.DesiredSize;
            }

            var firstRowIndex = Math.Max((int)(context.RealizationRect.Y / (MinimumItemSize.Height + RowSpacing)) - 1, 0);
            var lastRowIndex = Math.Min((int)(context.RealizationRect.Bottom / (MinimumItemSize.Height + RowSpacing)) + 1, context.ItemCount / 3);

            state.LayoutRectangleList.Clear();

            state.FirstRealizedIndex = firstRowIndex * 3;

            var desiredItemWidth = Math.Max(MinimumItemSize.Width, (availableSize.Width - ColumnSpacing * 3) / 4);

            for (var rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)
            {
                var firstItemIndex = rowIndex * 3;

                var currentRowRectangleArray = CalculateLayoutRectangleArrayForRow(rowIndex, desiredItemWidth);

                for (var columnIndex = 0; columnIndex < 3; columnIndex++)
                {
                    var index = firstItemIndex + columnIndex;
                    var currentRowRectangle = currentRowRectangleArray[index % 3];
                    var containerElement = context.GetOrCreateElementAt(index);

                    containerElement.Measure
                    (
                        new Size
                        (
                            currentRowRectangleArray[columnIndex].Width,
                            currentRowRectangleArray[columnIndex].Height
                        )
                    );

                    state.LayoutRectangleList.Add(currentRowRectangleArray[columnIndex]);
                }
            }

            var extentHeight = (context.ItemCount / 3 - 1) * (MinimumItemSize.Height + RowSpacing) + MinimumItemSize.Height;

            return new Size(desiredItemWidth * 4 + ColumnSpacing * 2, extentHeight);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            var state = (CustomVirtualizingLayoutState)context.LayoutState!;
            var currentIndex = state.FirstRealizedIndex;

            foreach (var layoutRectangle in state.LayoutRectangleList)
            {
                var containerElement = context.GetOrCreateElementAt(currentIndex);

                containerElement.Arrange(layoutRectangle);

                currentIndex++;
            }

            return finalSize;
        }

        private Rect[] CalculateLayoutRectangleArrayForRow(int rowIndex, double desiredItemWidth)
        {
            var rectangleArrayForRow = new Rect[3];
            var yOffset = rowIndex * (MinimumItemSize.Height + RowSpacing);

            rectangleArrayForRow[0] = rectangleArrayForRow[0].WithY(yOffset);
            rectangleArrayForRow[1] = rectangleArrayForRow[1].WithY(yOffset);
            rectangleArrayForRow[2] = rectangleArrayForRow[2].WithY(yOffset);

            rectangleArrayForRow[0] = rectangleArrayForRow[0].WithHeight(MinimumItemSize.Height);
            rectangleArrayForRow[1] = rectangleArrayForRow[1].WithHeight(MinimumItemSize.Height);
            rectangleArrayForRow[2] = rectangleArrayForRow[2].WithHeight(MinimumItemSize.Height);

            if (rowIndex % 2 == 0)
            {
                rectangleArrayForRow[0] = rectangleArrayForRow[0].WithX(0);
                rectangleArrayForRow[0] = rectangleArrayForRow[0].WithWidth(desiredItemWidth);

                rectangleArrayForRow[1] = rectangleArrayForRow[1].WithX(rectangleArrayForRow[0].Right + ColumnSpacing);
                rectangleArrayForRow[1] = rectangleArrayForRow[1].WithWidth(desiredItemWidth);

                rectangleArrayForRow[2] = rectangleArrayForRow[2].WithX(rectangleArrayForRow[1].Right + ColumnSpacing);
                rectangleArrayForRow[2] = rectangleArrayForRow[2].WithWidth(desiredItemWidth * 2 + ColumnSpacing);
            }
            else
            {
                rectangleArrayForRow[0] = rectangleArrayForRow[0].WithX(0);
                rectangleArrayForRow[0] = rectangleArrayForRow[0].WithWidth((desiredItemWidth * 2 + ColumnSpacing));

                rectangleArrayForRow[1] = rectangleArrayForRow[1].WithX(rectangleArrayForRow[0].Right + ColumnSpacing);
                rectangleArrayForRow[1] = rectangleArrayForRow[1].WithWidth(desiredItemWidth);

                rectangleArrayForRow[2] = rectangleArrayForRow[2].WithX(rectangleArrayForRow[1].Right + ColumnSpacing);
                rectangleArrayForRow[2] = rectangleArrayForRow[2].WithWidth(desiredItemWidth);
            }

            return rectangleArrayForRow;
        }
    }
}
