using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class MapButtonTests
    {
        [Fact]
        public void A_window_open_over_the_map_takes_the_mouse_wherever_the_cursor_is()
        {
            Assert.True(MapButton.TakesTheMouse(windowOpen: true));
        }

        [Fact]
        public void With_no_window_open_the_map_layer_takes_nothing_beyond_the_clicks_the_game_finds_on_the_button()
        {
            Assert.False(MapButton.TakesTheMouse(windowOpen: false));
        }
    }
}
