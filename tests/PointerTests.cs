using TradeLord;
using Xunit;

namespace TradeLord.Tests
{
    public class PointerTests
    {
        private const int Normal = 1;
        private const int Forbidden = 10;
        private const long NormalLook = 1001L;
        private const long ForbiddenLook = 2002L;
        private const float Settle = 0.1f;

        private static PointerLooks Taught(int shape, long look, float askedSince, float shownSince)
        {
            var looks = new PointerLooks();
            for (int i = 0; i < PointerLooks.KnownAfter; i++)
                looks.See(shape, look, 5f + i, askedSince, shownSince, Settle);
            return looks;
        }

        [Fact]
        public void A_pointer_is_learned_once_Windows_shows_it_after_the_game_asks_for_it()
        {
            var looks = Taught(Normal, NormalLook, askedSince: 1f, shownSince: 1.02f);
            Assert.True(looks.Known(Normal, out long look));
            Assert.Equal(NormalLook, look);
        }

        [Fact]
        public void A_pointer_Windows_showed_before_the_game_asked_teaches_nothing()
        {
            var looks = Taught(Normal, ForbiddenLook, askedSince: 2f, shownSince: 1f);
            Assert.False(looks.Known(Normal, out _));
        }

        [Fact]
        public void A_pointer_is_not_learned_before_the_game_and_Windows_have_both_held_it_a_moment()
        {
            var looks = new PointerLooks();
            for (int i = 0; i < PointerLooks.KnownAfter * 2; i++)
                looks.See(Normal, NormalLook, 1.05f, 1f, 1f, Settle);
            Assert.False(looks.Known(Normal, out _));
        }

        [Fact]
        public void A_pointer_needs_enough_frames_before_it_counts_as_known()
        {
            var looks = new PointerLooks();
            for (int i = 0; i < PointerLooks.KnownAfter - 1; i++)
                looks.See(Normal, NormalLook, 5f + i, 1f, 1f, Settle);
            Assert.False(looks.Known(Normal, out _));
            looks.See(Normal, NormalLook, 20f, 1f, 1f, Settle);
            Assert.True(looks.Known(Normal, out _));
        }

        [Fact]
        public void A_held_forbidden_sign_cannot_teach_the_normal_pointer_another_look()
        {
            var looks = Taught(Normal, NormalLook, 1f, 1f);
            for (int i = 0; i < PointerLooks.KnownAfter * 5; i++)
                looks.See(Normal, ForbiddenLook, 50f + i, 30f, 40f, Settle);
            Assert.True(looks.Known(Normal, out long look));
            Assert.Equal(NormalLook, look);
        }

        [Fact]
        public void Each_look_is_named_by_the_pointer_the_game_asked_for()
        {
            var looks = Taught(Normal, NormalLook, 1f, 1f);
            for (int i = 0; i < PointerLooks.KnownAfter; i++)
                looks.See(Forbidden, ForbiddenLook, 60f + i, 50f, 50.05f, Settle);
            Assert.True(looks.ShapeOf(ForbiddenLook, out int forbidden));
            Assert.Equal(Forbidden, forbidden);
            Assert.True(looks.ShapeOf(NormalLook, out int normal));
            Assert.Equal(Normal, normal);
            Assert.False(looks.ShapeOf(3003L, out _));
        }

        [Fact]
        public void Windows_is_wrong_only_once_the_game_has_settled_on_a_pointer_whose_look_is_known()
        {
            Assert.True(PointerHeld.Wrong(settled: true, known: true, ForbiddenLook, NormalLook));
            Assert.False(PointerHeld.Wrong(settled: false, known: true, ForbiddenLook, NormalLook));
            Assert.False(PointerHeld.Wrong(settled: true, known: false, ForbiddenLook, NormalLook));
            Assert.False(PointerHeld.Wrong(settled: true, known: true, NormalLook, NormalLook));
        }

        [Fact]
        public void A_held_sign_lasts_while_Windows_keeps_it_and_the_game_asks_for_something_else()
        {
            Assert.True(PointerHeld.Still(ForbiddenLook, ForbiddenLook, settled: true, known: true, NormalLook));
            Assert.True(PointerHeld.Still(ForbiddenLook, ForbiddenLook, settled: false, known: true, ForbiddenLook));
            Assert.True(PointerHeld.Still(ForbiddenLook, ForbiddenLook, settled: true, known: false, 0L));
        }

        [Fact]
        public void A_held_sign_ends_once_Windows_shows_another_look_or_the_game_asks_for_that_sign_itself()
        {
            Assert.False(PointerHeld.Still(NormalLook, ForbiddenLook, settled: true, known: true, NormalLook));
            Assert.False(PointerHeld.Still(ForbiddenLook, ForbiddenLook, settled: true, known: true, ForbiddenLook));
        }
    }
}
