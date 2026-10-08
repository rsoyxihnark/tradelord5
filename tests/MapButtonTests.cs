using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class MapButtonTests
    {
        [Fact]
        public void A_window_open_over_the_map_shows_the_mouse_and_takes_the_wheel_wherever_the_cursor_is()
        {
            var takes = MapButton.LayerTakes(windowOpen: true);
            Assert.True(takes.showsMouse);
            Assert.True(takes.takesWheel);
        }

        [Fact]
        public void With_no_window_open_TradeLords_layer_never_shows_the_mouse_and_leaves_the_wheel_to_the_map()
        {
            var takes = MapButton.LayerTakes(windowOpen: false);
            Assert.False(takes.showsMouse);
            Assert.False(takes.takesWheel);
        }
    }
}
