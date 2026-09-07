using Xunit;

namespace TradeLord.Tests
{
    public class BooksTests
    {
        private static Books Fresh() => new Books();

        [Fact]
        public void ARealRunReadsNothingBackFromTheDryRunBooks()
        {
            Books books = Fresh();
            books.NotePurchase("grain", 40, 1.5f, 1);
            books.NoteSale("wool", 30, 2f, 0);
            books.NoteShed(haulAnimal: true, spareMount: false);
            books.NoteHerdTaken();

            Assert.Equal(0, books.PaidOut(false));
            Assert.Equal(0, books.Purse(false));
            Assert.Equal(0, books.TillDrawn(false));
            Assert.Equal(0f, books.Weight(false));
            Assert.Equal(0, books.FoodHeld(false));
            Assert.Equal(0, books.Shed(false));
            Assert.Equal(0, books.MountsShed(false));
            Assert.Equal(0, books.HaulsShed(false));
            Assert.Equal(0, books.HerdTaken(false));
            Assert.Equal(0, books.Held(false, "grain"));
            Assert.Equal(0, books.Stocked(false, "grain"));
            Assert.False(books.Sold(false, "wool"));
            Assert.False(books.Bought(false, "grain"));
            Assert.False(books.Traded(false));
            Assert.Equal((0, 0), books.Purchases(false, "grain"));
        }

        [Fact]
        public void WhatOnePassSpendsIsGoneFromTheNextPassesPurse()
        {
            Books books = Fresh();
            books.NotePurchase("grain", 40, 1f, 1);
            books.NotePurchase("grain", 45, 1f, 1);
            books.NotePurchase("iron", 200, 10f, 0);

            Assert.Equal(285, books.PaidOut(true));
            Assert.Equal(-285, books.Purse(true));
        }

        [Fact]
        public void WhatOnePassSellsIsInTheNextPassesPurseAndOutOfTheMerchantsTill()
        {
            Books books = Fresh();
            books.NoteSale("wool", 120, 2f, 0);
            books.NoteSale("wool", 110, 2f, 0);

            Assert.Equal(230, books.Purse(true));
            Assert.Equal(230, books.TillDrawn(true));
            Assert.Equal(0, books.PaidOut(true));
        }

        [Fact]
        public void SellingThenBuyingLeavesThePurseAtTheDifference()
        {
            Books books = Fresh();
            books.NoteSale("wool", 300, 2f, 0);
            books.NotePurchase("iron", 180, 10f, 0);

            Assert.Equal(120, books.Purse(true));
            Assert.Equal(180, books.PaidOut(true));
            Assert.Equal(300, books.TillDrawn(true));
        }

        [Fact]
        public void CargoRoomIsGivenUpOnABuyAndHandedBackOnASale()
        {
            Books books = Fresh();
            books.NotePurchase("iron", 100, 10f, 0);
            Assert.Equal(10f, books.Weight(true));

            books.NoteSale("wool", 50, 4f, 0);
            Assert.Equal(6f, books.Weight(true));
        }

        [Fact]
        public void TheLarderCountsWhatTheVisitHasAlreadyBoughtAndSold()
        {
            Books books = Fresh();
            books.NotePurchase("grain", 40, 1f, 1);
            books.NotePurchase("grain", 40, 1f, 1);
            Assert.Equal(2, books.FoodHeld(true));

            books.NoteSale("cow", 90, 5f, 3);
            Assert.Equal(-1, books.FoodHeld(true));
        }

        [Fact]
        public void AGoodBoughtThisVisitIsOffTheShelfAndInTheParty()
        {
            Books books = Fresh();
            books.NotePurchase("grain", 40, 1f, 1);
            books.NotePurchase("grain", 42, 1f, 1);

            Assert.Equal(2, books.Stocked(true, "grain"));
            Assert.Equal(2, books.Held(true, "grain"));
            Assert.True(books.Bought(true, "grain"));
            Assert.Equal((2, 82), books.Purchases(true, "grain"));
            Assert.Equal(0, books.Stocked(true, "iron"));
            Assert.Equal((0, 0), books.Purchases(true, "iron"));
        }

        [Fact]
        public void AGoodSoldThisVisitIsOutOfThePartyAndNotOfferedBack()
        {
            Books books = Fresh();
            books.NoteSale("wool", 120, 2f, 0);
            books.NoteSale("wool", 110, 2f, 0);

            Assert.Equal(-2, books.Held(true, "wool"));
            Assert.True(books.Sold(true, "wool"));
            Assert.False(books.Sold(true, "iron"));
            Assert.Equal(0, books.Stocked(true, "wool"));
        }

        [Fact]
        public void AnAnimalTheDryRunHasAlreadyShedIsNotShedTwice()
        {
            Books books = Fresh();
            books.NoteShed(haulAnimal: false, spareMount: false);
            books.NoteShed(haulAnimal: false, spareMount: true);
            books.NoteShed(haulAnimal: true, spareMount: false);

            Assert.Equal(3, books.Shed(true));
            Assert.Equal(1, books.MountsShed(true));
            Assert.Equal(1, books.HaulsShed(true));
        }

        [Fact]
        public void AHaulAnimalIsCountedOnceAndNeverAlsoAsASpareMount()
        {
            Books books = Fresh();
            books.NoteShed(haulAnimal: true, spareMount: true);

            Assert.Equal(1, books.Shed(true));
            Assert.Equal(1, books.HaulsShed(true));
            Assert.Equal(0, books.MountsShed(true));
        }

        [Fact]
        public void EveryAnimalTakenIntoTheHerdCountsAgainstTheRoomLeft()
        {
            Books books = Fresh();
            books.NoteHerdTaken();
            books.NoteHerdTaken();

            Assert.Equal(2, books.HerdTaken(true));
        }

        [Fact]
        public void AVisitThatMovedNothingSaysSo()
        {
            Books books = Fresh();
            Assert.False(books.Traded(true));

            books.NoteSale("wool", 10, 1f, 0);
            Assert.True(books.Traded(true));

            books = Fresh();
            books.NotePurchase("grain", 10, 1f, 1);
            Assert.True(books.Traded(true));
        }

        [Fact]
        public void AGoodWithNoNameIsIgnoredRatherThanCounted()
        {
            Books books = Fresh();
            books.NoteSale(null, 100, 5f, 2);
            books.NotePurchase(null, 100, 5f, 2);

            Assert.Equal(0, books.Purse(true));
            Assert.Equal(0, books.PaidOut(true));
            Assert.Equal(0, books.TillDrawn(true));
            Assert.Equal(0f, books.Weight(true));
            Assert.Equal(0, books.FoodHeld(true));
            Assert.False(books.Traded(true));
            Assert.Equal(0, books.Held(true, null));
            Assert.Equal(0, books.Stocked(true, null));
            Assert.False(books.Sold(true, null));
            Assert.False(books.Bought(true, null));
            Assert.Equal((0, 0), books.Purchases(true, null));
        }

        [Fact]
        public void TheNextVisitStartsWithNothingCarriedOverFromTheLast()
        {
            Books books = Fresh();
            books.NoteSale("wool", 120, 2f, 1);
            books.NotePurchase("grain", 40, 1f, 1);
            books.NoteShed(haulAnimal: true, spareMount: false);
            books.NoteShed(haulAnimal: false, spareMount: true);
            books.NoteHerdTaken();

            books.Forget();

            Assert.Equal(0, books.PaidOut(true));
            Assert.Equal(0, books.Purse(true));
            Assert.Equal(0, books.TillDrawn(true));
            Assert.Equal(0f, books.Weight(true));
            Assert.Equal(0, books.FoodHeld(true));
            Assert.Equal(0, books.Shed(true));
            Assert.Equal(0, books.MountsShed(true));
            Assert.Equal(0, books.HaulsShed(true));
            Assert.Equal(0, books.HerdTaken(true));
            Assert.Equal(0, books.Held(true, "wool"));
            Assert.Equal(0, books.Stocked(true, "grain"));
            Assert.False(books.Sold(true, "wool"));
            Assert.False(books.Bought(true, "grain"));
            Assert.False(books.Traded(true));
        }

        [Fact]
        public void WhatAPassReallyMovedIsReadBackOnADryRunAndARealRunAlike()
        {
            Books books = Fresh();
            books.NoteSold("wool");
            books.NoteBought("grain", 40);
            books.NoteBought("grain", 44);

            Assert.True(books.Sold(false, "wool"));
            Assert.True(books.Sold(true, "wool"));
            Assert.True(books.Bought(false, "grain"));
            Assert.True(books.Bought(true, "grain"));
            Assert.True(books.Traded(false));
            Assert.Equal((2, 84), books.Purchases(false, "grain"));
            Assert.Equal(84, books.PaidOut(false));
        }

        [Fact]
        public void ADryRunAddsToWhatTheVisitAlreadyBoughtForReal()
        {
            Books books = Fresh();
            books.NoteBought("grain", 40);
            books.NotePurchase("grain", 44, 1f, 1);

            Assert.Equal((1, 40), books.Purchases(false, "grain"));
            Assert.Equal((2, 84), books.Purchases(true, "grain"));
            Assert.Equal(40, books.PaidOut(false));
            Assert.Equal(84, books.PaidOut(true));
        }

        [Fact]
        public void OnePassesBooksAreNotTheNextPassesBooks()
        {
            Books market = Fresh();
            Books road = Fresh();
            market.NoteSold("wool");
            market.NoteBought("grain", 40);

            Assert.False(road.Sold(true, "wool"));
            Assert.False(road.Bought(true, "grain"));
            Assert.False(road.Traded(true));
            Assert.Equal(0, road.PaidOut(true));
        }
    }
}
