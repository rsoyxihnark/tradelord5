using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class MapButtonTests
    {
        private const float ScreenW = 1920f, ScreenH = 1080f;
        private const float Width = 50f, Height = 50f, MarginRight = 20f;

        private static bool Over(float x, float y) =>
            MapButton.Over(x, y, ScreenW, ScreenH, Width, Height, MarginRight);

        private static float Right => 1f - MarginRight / ScreenW;
        private static float Left => Right - Width / ScreenW;
        private static float Half => Height / ScreenH * 0.5f;

        [Fact]
        public void The_middle_of_the_button_is_over_the_button()
        {
            Assert.True(Over((Left + Right) * 0.5f, 0.5f));
        }

        [Fact]
        public void The_middle_of_the_map_is_not()
        {
            Assert.False(Over(0.5f, 0.5f));
            Assert.False(Over(0.1f, 0.5f));
        }

        [Fact]
        public void The_button_sits_flush_right_and_centred()
        {
            Assert.True(Over(Right, 0.5f));
            Assert.True(Over(Left, 0.5f));
            Assert.True(Over((Left + Right) * 0.5f, 0.5f - Half));
            Assert.True(Over((Left + Right) * 0.5f, 0.5f + Half));
        }

        [Fact]
        public void A_cursor_just_outside_is_still_caught_and_one_further_out_is_not()
        {
            float padX = MapButton.Pad / ScreenW, padY = MapButton.Pad / ScreenH;
            Assert.True(Over(Left - padX * 0.9f, 0.5f));
            Assert.False(Over(Left - padX * 1.1f, 0.5f));
            Assert.True(Over((Left + Right) * 0.5f, 0.5f - Half - padY * 0.9f));
            Assert.False(Over((Left + Right) * 0.5f, 0.5f - Half - padY * 1.1f));
        }

        [Fact]
        public void A_screen_or_a_button_the_game_cannot_measure_is_not_readable()
        {
            Assert.False(MapButton.BoundsReadable(0f, ScreenH, Width, Height));
            Assert.False(MapButton.BoundsReadable(ScreenW, 0f, Width, Height));
            Assert.False(MapButton.BoundsReadable(ScreenW, ScreenH, 0f, Height));
            Assert.False(MapButton.BoundsReadable(ScreenW, ScreenH, Width, 0f));
            Assert.False(MapButton.BoundsReadable(ScreenW, ScreenH, ScreenW + 1f, Height));
            Assert.False(MapButton.BoundsReadable(ScreenW, ScreenH, Width, ScreenH + 1f));
            Assert.True(MapButton.BoundsReadable(ScreenW, ScreenH, Width, Height));
        }

        [Fact]
        public void Nothing_is_over_a_button_that_cannot_be_measured()
        {
            Assert.False(MapButton.Over(0.97f, 0.5f, 0f, 0f, Width, Height, MarginRight));
            Assert.False(MapButton.Over(0.97f, 0.5f, ScreenW, ScreenH, ScreenW + 1f, Height, MarginRight));
        }

        [Fact]
        public void No_part_of_the_map_is_reserved_when_the_button_cannot_be_measured()
        {
            foreach (float x in new[] { 0.5f, 0.90f, 0.95f, 0.99f })
                foreach (float y in new[] { 0.2f, 0.46f, 0.5f, 0.54f, 0.8f })
                {
                    Assert.False(MapButton.Over(x, y, ScreenW, ScreenH, 0f, 0f, MarginRight));
                    Assert.False(MapButton.Over(x, y, 0f, 0f, Width, Height, MarginRight));
                }
        }

        [Fact]
        public void A_window_open_over_the_map_takes_the_mouse_wherever_the_cursor_is()
        {
            Assert.True(MapButton.TakesTheMouse(windowOpen: true, buttonOn: true, overButton: false));
            Assert.True(MapButton.TakesTheMouse(windowOpen: true, buttonOn: false, overButton: false));
        }

        [Fact]
        public void With_no_window_open_only_the_cursor_on_the_button_takes_the_mouse()
        {
            Assert.True(MapButton.TakesTheMouse(windowOpen: false, buttonOn: true, overButton: true));
            Assert.False(MapButton.TakesTheMouse(windowOpen: false, buttonOn: true, overButton: false));
            Assert.False(MapButton.TakesTheMouse(windowOpen: false, buttonOn: false, overButton: true));
        }

        [Fact]
        public void The_region_holds_at_any_aspect_ratio()
        {
            var rng = new System.Random(8890);
            for (int round = 0; round < 20000; round++)
            {
                float w = rng.Next(640, 3841);
                float h = rng.Next(480, 2161);
                float bw = rng.Next(20, 200);
                float bh = rng.Next(20, 200);
                float margin = rng.Next(0, 100);
                if (!MapButton.BoundsReadable(w, h, bw, bh)) continue;
                float right = 1f - margin / w;
                float left = right - bw / w;
                Assert.True(MapButton.Over((left + right) * 0.5f, 0.5f, w, h, bw, bh, margin));
                Assert.False(MapButton.Over(0.2f, 0.5f, w, h, bw, bh, margin));
            }
        }
    }
}
