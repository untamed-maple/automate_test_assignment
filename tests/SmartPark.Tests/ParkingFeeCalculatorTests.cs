using SmartPark.Core.Models;
using SmartPark.Core.Services;
using FsCheck;
using FsCheck.Xunit;

namespace SmartPark.Tests;

public class ParkingFeeCalculatorTests
{
    private readonly ParkingFeeCalculator _calculator = new();

    // ────────────────────────────────────────────────────────────
    //  EXAMPLE TEST — shows the naming convention and AAA pattern.
    //  Delete or keep this; it does not count toward your grade.
    // ────────────────────────────────────────────────────────────

    [Fact]
    public void CalculateFee_ZeroDuration_ReturnsFree()
    {
        
        // Arrange
        var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);  // Monday
        var checkOut = checkIn; // same time = 0 duration

        // Act
        var result = _calculator.CalculateFee(VehicleType.Car, MembershipTier.Guest, checkIn, checkOut);

        // Assert
        Assert.Equal(0m, result.TotalFee);
    }

    #region Basic Fee Calculation
    // Test basic hourly rates for each vehicle type
    // Consider using [Theory] with [InlineData] for multiple scenarios
    #endregion

    #region Grace Period
    // Test the free parking window and its boundaries
    [Fact]
public void CalculateFee_ThirtyOneMinutes_ChargesOneHour()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
    var checkOut = checkIn.AddMinutes(31);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        checkOut);

    // Assert
    Assert.Equal(1000m, result.TotalFee);
}
    #endregion

    #region Duration Rounding
    // Test how partial hours are rounded for billing
    #endregion

    #region Daily Cap
    // Test that fees respect maximum daily limits per vehicle type
    [Fact]
public void CalculateFee_MotorcycleAtExactDailyCap_Returns4000()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 8, 0, 0);
    var checkOut = checkIn.AddHours(8);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Motorcycle,
        MembershipTier.Guest,
        checkIn,
        checkOut);

    // Assert
    Assert.Equal(4000m, result.TotalFee);
}
    #endregion
    [Fact]
public void CalculateFee_MotorcycleExceedsDailyCap_ReturnsCappedFee()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 8, 0, 0);
    var checkOut = checkIn.AddHours(12);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Motorcycle,
        MembershipTier.Guest,
        checkIn,
        checkOut);

    // Assert
    Assert.Equal(4000m, result.TotalFee);
}
    

    #region Overnight Fee
    [Fact]
public void CalculateFee_OvernightParking_AddsOvernightFee()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 20, 0, 0); // 8 PM
    var checkOut = new DateTime(2026, 3, 16, 23, 0, 0); // 11 PM

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        checkOut);

    // Assert
    Assert.Equal(5000m, result.TotalFee);
}
    #endregion

    #region Weekend Surcharge
    // Test the percentage-based surcharge on specific days
    [Fact]
public void CalculateFee_WeekendParking_AddsTwentyPercentSurcharge()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 14, 10, 0, 0); // Saturday
    var checkOut = checkIn.AddHours(2);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        checkOut);

    // Assert
    Assert.Equal(2400m, result.TotalFee);
    
}

    #endregion

    #region Holiday Surcharge
    // Test holiday pricing and its interaction with weekend pricing
    [Fact]
public void CalculateFee_HolidayParking_OverridesWeekendSurcharge()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 14, 10, 0, 0); // Saturday
    var checkOut = checkIn.AddHours(2);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        checkOut,
        isHoliday: true);

    // Assert
    Assert.Equal(3000m, result.TotalFee);
}
    #endregion

    #region Membership Discounts
    // Test discount tiers and what amounts they apply to
    [Fact]
public void CalculateFee_GoldMember_AppliesTwentyFivePercentDiscount()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
    var checkOut = checkIn.AddHours(2);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Gold,
        checkIn,
        checkOut);

    // Assert
    Assert.Equal(1500m, result.TotalFee);
}
    #endregion

    #region Lost Ticket
    // Test the penalty and how it interacts with other fee modifiers
    [Fact]
public void CalculateFee_LostTicket_AddsPenaltyFee()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
    var checkOut = checkIn.AddHours(2);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        checkOut,
        isLostTicket: true);

    // Assert
    Assert.Equal(12000m, result.TotalFee);
}
    #endregion

    #region Edge Cases
    // Test invalid inputs and boundary conditions
    [Fact]
public void CalculateFee_CheckOutBeforeCheckIn_ThrowsException()
{
    // Arrange
    var checkIn = new DateTime(2026, 3, 16, 12, 0, 0);
    var checkOut = new DateTime(2026, 3, 16, 10, 0, 0);

    // Act + Assert
    Assert.Throws<ArgumentException>(() =>
        _calculator.CalculateFee(
            VehicleType.Car,
            MembershipTier.Guest,
            checkIn,
            checkOut));
}
    #endregion

    #region Property-Based Tests
    // Write at least 5 FsCheck properties that must hold for ALL valid inputs
    // You may need custom Arbitrary<T> for generating valid DateTime pairs
    [Property]
public void CalculateFee_FeeIsNeverNegative(int parkingMinutes)
{
    // Arrange
    parkingMinutes = Math.Abs(parkingMinutes % 1440);

    var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);
    var checkOut = checkIn.AddMinutes(parkingMinutes);

    // Act
    var result = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        checkOut);

    // Assert
    Assert.True(result.TotalFee >= 0);
}
[Property]
public void CalculateFee_LongerDurationDoesNotReduceFee(
    int firstMinutes,
    int extraMinutes)
{
    // Arrange
    firstMinutes = Math.Abs(firstMinutes % 720);
    extraMinutes = Math.Abs(extraMinutes % 720);

    var checkIn = new DateTime(2026, 3, 16, 10, 0, 0);

    var shorterCheckOut = checkIn.AddMinutes(firstMinutes);
    var longerCheckOut = shorterCheckOut.AddMinutes(extraMinutes);

    // Act
    var shorterFee = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        shorterCheckOut);

    var longerFee = _calculator.CalculateFee(
        VehicleType.Car,
        MembershipTier.Guest,
        checkIn,
        longerCheckOut);

    // Assert
    Assert.True(longerFee.TotalFee >= shorterFee.TotalFee);
}

    #endregion
}
