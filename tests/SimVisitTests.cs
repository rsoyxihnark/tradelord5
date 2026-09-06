using Xunit;

namespace TradeLord.Tests
{
    public class SimVisitTests
    {
        private static void Fresh() => SimVisit.Forget();

        [Fact]
        public void ARealRunReadsNothingBackFromTheDryRunLedger()
        {
            Fresh();
            SimVisit.NotePurchase("grain", 40, 1.5f, 1);
            SimVisit.NoteSale("wool", 30, 2f, 0);
            SimVisit.NoteShed(haulAnimal: true, spareMount: false);
            SimVisit.NoteHerdTaken();

            Assert.Equal(0, SimVisit.Spent(false));
            Assert.Equal(0, SimVisit.Purse(false));
            Assert.Equal(0, SimVisit.TillDrawn(false));
            Assert.Equal(0f, SimVisit.Weight(false));
            Assert.Equal(0, SimVisit.FoodHeld(false));
            Assert.Equal(0, SimVisit.Shed(false));
            Assert.Equal(0, SimVisit.MountsShed(false));
            Assert.Equal(0, SimVisit.HaulsShed(false));
            Assert.Equal(0, SimVisit.HerdTaken(false));
            Assert.Equal(0, SimVisit.Held(false, "grain"));
            Assert.Equal(0, SimVisit.Stocked(false, "grain"));
            Assert.False(SimVisit.Sold(false, "wool"));
            Assert.False(SimVisit.Bought(false, "grain"));
            Assert.False(SimVisit.Traded(false));
            Assert.Equal((0, 0), SimVisit.Purchases(false, "grain"));
        }

        [Fact]
        public void WhatOnePassSpendsIsGoneFromTheNextPassesPurse()
        {
            Fresh();
            SimVisit.NotePurchase("grain", 40, 1f, 1);
            SimVisit.NotePurchase("grain", 45, 1f, 1);
            SimVisit.NotePurchase("iron", 200, 10f, 0);

            Assert.Equal(285, SimVisit.Spent(true));
            Assert.Equal(-285, SimVisit.Purse(true));
        }

        [Fact]
        public void WhatOnePassSellsIsInTheNextPassesPurseAndOutOfTheMerchantsTill()
        {
            Fresh();
            SimVisit.NoteSale("wool", 120, 2f, 0);
            SimVisit.NoteSale("wool", 110, 2f, 0);

            Assert.Equal(230, SimVisit.Purse(true));
            Assert.Equal(230, SimVisit.TillDrawn(true));
            Assert.Equal(0, SimVisit.Spent(true));
        }

        [Fact]
        public void SellingThenBuyingLeavesThePurseAtTheDifference()
        {
            Fresh();
            SimVisit.NoteSale("wool", 300, 2f, 0);
            SimVisit.NotePurchase("iron", 180, 10f, 0);

            Assert.Equal(120, SimVisit.Purse(true));
            Assert.Equal(180, SimVisit.Spent(true));
            Assert.Equal(300, SimVisit.TillDrawn(true));
        }

        [Fact]
        public void CargoRoomIsGivenUpOnABuyAndHandedBackOnASale()
        {
            Fresh();
            SimVisit.NotePurchase("iron", 100, 10f, 0);
            Assert.Equal(10f, SimVisit.Weight(true));

            SimVisit.NoteSale("wool", 50, 4f, 0);
            Assert.Equal(6f, SimVisit.Weight(true));
        }

        [Fact]
        public void TheLarderCountsWhatTheVisitHasAlreadyBoughtAndSold()
        {
            Fresh();
            SimVisit.NotePurchase("grain", 40, 1f, 1);
            SimVisit.NotePurchase("grain", 40, 1f, 1);
            Assert.Equal(2, SimVisit.FoodHeld(true));

            SimVisit.NoteSale("cow", 90, 5f, 3);
            Assert.Equal(-1, SimVisit.FoodHeld(true));
        }

        [Fact]
        public void AGoodBoughtThisVisitIsOffTheShelfAndInTheParty()
        {
            Fresh();
            SimVisit.NotePurchase("grain", 40, 1f, 1);
            SimVisit.NotePurchase("grain", 42, 1f, 1);

            Assert.Equal(2, SimVisit.Stocked(true, "grain"));
            Assert.Equal(2, SimVisit.Held(true, "grain"));
            Assert.True(SimVisit.Bought(true, "grain"));
            Assert.Equal((2, 82), SimVisit.Purchases(true, "grain"));
            Assert.Equal(0, SimVisit.Stocked(true, "iron"));
            Assert.Equal((0, 0), SimVisit.Purchases(true, "iron"));
        }

        [Fact]
        public void AGoodSoldThisVisitIsOutOfThePartyAndNotOfferedBack()
        {
            Fresh();
            SimVisit.NoteSale("wool", 120, 2f, 0);
            SimVisit.NoteSale("wool", 110, 2f, 0);

            Assert.Equal(-2, SimVisit.Held(true, "wool"));
            Assert.True(SimVisit.Sold(true, "wool"));
            Assert.False(SimVisit.Sold(true, "iron"));
            Assert.Equal(0, SimVisit.Stocked(true, "wool"));
        }

        [Fact]
        public void AnAnimalTheDryRunHasAlreadyShedIsNotShedTwice()
        {
            Fresh();
            SimVisit.NoteShed(haulAnimal: false, spareMount: false);
            SimVisit.NoteShed(haulAnimal: false, spareMount: true);
            SimVisit.NoteShed(haulAnimal: true, spareMount: false);

            Assert.Equal(3, SimVisit.Shed(true));
            Assert.Equal(1, SimVisit.MountsShed(true));
            Assert.Equal(1, SimVisit.HaulsShed(true));
        }

        [Fact]
        public void AHaulAnimalIsCountedOnceAndNeverAlsoAsASpareMount()
        {
            Fresh();
            SimVisit.NoteShed(haulAnimal: true, spareMount: true);

            Assert.Equal(1, SimVisit.Shed(true));
            Assert.Equal(1, SimVisit.HaulsShed(true));
            Assert.Equal(0, SimVisit.MountsShed(true));
        }

        [Fact]
        public void EveryAnimalTakenIntoTheHerdCountsAgainstTheRoomLeft()
        {
            Fresh();
            SimVisit.NoteHerdTaken();
            SimVisit.NoteHerdTaken();

            Assert.Equal(2, SimVisit.HerdTaken(true));
        }

        [Fact]
        public void AVisitThatMovedNothingSaysSo()
        {
            Fresh();
            Assert.False(SimVisit.Traded(true));

            SimVisit.NoteSale("wool", 10, 1f, 0);
            Assert.True(SimVisit.Traded(true));

            Fresh();
            SimVisit.NotePurchase("grain", 10, 1f, 1);
            Assert.True(SimVisit.Traded(true));
        }

        [Fact]
        public void AGoodWithNoNameIsIgnoredRatherThanCounted()
        {
            Fresh();
            SimVisit.NoteSale(null, 100, 5f, 2);
            SimVisit.NotePurchase(null, 100, 5f, 2);

            Assert.Equal(0, SimVisit.Purse(true));
            Assert.Equal(0, SimVisit.Spent(true));
            Assert.Equal(0, SimVisit.TillDrawn(true));
            Assert.Equal(0f, SimVisit.Weight(true));
            Assert.Equal(0, SimVisit.FoodHeld(true));
            Assert.False(SimVisit.Traded(true));
            Assert.Equal(0, SimVisit.Held(true, null));
            Assert.Equal(0, SimVisit.Stocked(true, null));
            Assert.False(SimVisit.Sold(true, null));
            Assert.False(SimVisit.Bought(true, null));
            Assert.Equal((0, 0), SimVisit.Purchases(true, null));
        }

        [Fact]
        public void TheNextVisitStartsWithNothingCarriedOverFromTheLast()
        {
            Fresh();
            SimVisit.NoteSale("wool", 120, 2f, 1);
            SimVisit.NotePurchase("grain", 40, 1f, 1);
            SimVisit.NoteShed(haulAnimal: true, spareMount: false);
            SimVisit.NoteShed(haulAnimal: false, spareMount: true);
            SimVisit.NoteHerdTaken();

            SimVisit.Forget();

            Assert.Equal(0, SimVisit.Spent(true));
            Assert.Equal(0, SimVisit.Purse(true));
            Assert.Equal(0, SimVisit.TillDrawn(true));
            Assert.Equal(0f, SimVisit.Weight(true));
            Assert.Equal(0, SimVisit.FoodHeld(true));
            Assert.Equal(0, SimVisit.Shed(true));
            Assert.Equal(0, SimVisit.MountsShed(true));
            Assert.Equal(0, SimVisit.HaulsShed(true));
            Assert.Equal(0, SimVisit.HerdTaken(true));
            Assert.Equal(0, SimVisit.Held(true, "wool"));
            Assert.Equal(0, SimVisit.Stocked(true, "grain"));
            Assert.False(SimVisit.Sold(true, "wool"));
            Assert.False(SimVisit.Bought(true, "grain"));
            Assert.False(SimVisit.Traded(true));
        }
    }
}
