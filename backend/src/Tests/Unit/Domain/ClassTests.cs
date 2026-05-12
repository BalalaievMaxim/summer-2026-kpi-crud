using FluentAssertions;
using GymManagement.Domain.Classes;
using GymManagement.Domain.Classes.Errors;
using GymManagement.Domain.Shared.ValueObjects;

namespace GymManagement.Tests.Unit.Domain;

public class ClassTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private static readonly DateTimeOffset FutureStart = Now.AddDays(1);
    private static readonly DateTimeOffset FutureEnd = FutureStart.AddHours(2);

    private static Class CreateFutureClass(int capacity = 10, int id = 1)
        => Class.Create(id, classTypeId: 1, coachId: 1, FutureStart, FutureEnd, capacity);

    [Fact]
    public void Create_ValidData_ReturnsClass()
    {
        var cls = CreateFutureClass(capacity: 20, id: 5);

        cls.Id.Should().Be(5);
        cls.ClassTypeId.Should().Be(1);
        cls.CoachId.Should().Be(1);
        cls.Capacity.Should().Be(20);
        cls.Schedule.Should().NotBeNull();
        cls.Enrollments.Should().BeEmpty();
        cls.IsFull.Should().BeFalse();
    }

    [Fact]
    public void Create_ZeroCapacity_ThrowsInvalidCapacityError()
    {
        var act = () => Class.Create(1, 1, 1, FutureStart, FutureEnd, capacity: 0);
        act.Should().Throw<InvalidCapacityError>();
    }

    [Fact]
    public void Create_NegativeCapacity_ThrowsInvalidCapacityError()
    {
        var act = () => Class.Create(1, 1, 1, FutureStart, FutureEnd, capacity: -5);
        act.Should().Throw<InvalidCapacityError>();
    }

    [Fact]
    public void Create_CapacityAboveDatabaseLimit_ThrowsInvalidCapacityError()
    {
        var act = () => Class.Create(1, 1, 1, FutureStart, FutureEnd, capacity: Class.MaxCapacity + 1);
        act.Should().Throw<InvalidCapacityError>();
    }

    [Fact]
    public void Enroll_ValidClient_AddsEnrollment()
    {
        var cls = CreateFutureClass();

        var enrollment = cls.Enroll(clientId: 42, Now);

        cls.Enrollments.Should().HaveCount(1);
        enrollment.ClientId.Should().Be(42);
        enrollment.ClassId.Should().Be(cls.Id);
        enrollment.RegistrationTime.Should().Be(Now);
    }

    [Fact]
    public void Enroll_ClassFull_ThrowsClassFullError()
    {
        var cls = CreateFutureClass(capacity: 1);
        cls.Enroll(clientId: 1, Now);

        var act = () => cls.Enroll(clientId: 2, Now);
        act.Should().Throw<ClassFullError>();
    }

    [Fact]
    public void Enroll_ClassInPast_ThrowsClassInPastError()
    {
        var pastStart = Now.AddDays(-2);
        var pastEnd = pastStart.AddHours(1);
        var cls = Class.Create(1, 1, 1, pastStart, pastEnd, 10);

        var act = () => cls.Enroll(clientId: 1, Now);
        act.Should().Throw<ClassInPastError>();
    }

    [Fact]
    public void Enroll_DuplicateClient_ThrowsDuplicateEnrollmentError()
    {
        var cls = CreateFutureClass(capacity: 10);
        cls.Enroll(clientId: 5, Now);

        var act = () => cls.Enroll(clientId: 5, Now);
        act.Should().Throw<DuplicateEnrollmentError>();
    }

    [Fact]
    public void CancelEnrollment_ExistingClient_RemovesEnrollment()
    {
        var cls = CreateFutureClass();
        cls.Enroll(clientId: 10, Now);
        cls.Enrollments.Should().HaveCount(1);

        cls.CancelEnrollment(clientId: 10);

        cls.Enrollments.Should().BeEmpty();
    }

    [Fact]
    public void CancelEnrollment_NonExistingClient_ThrowsEnrollmentNotFoundInClassError()
    {
        var cls = CreateFutureClass();

        var act = () => cls.CancelEnrollment(clientId: 999);
        act.Should().Throw<EnrollmentNotFoundInClassError>();
    }

    [Fact]
    public void IsFull_AtCapacity_ReturnsTrue()
    {
        var cls = CreateFutureClass(capacity: 2);
        cls.Enroll(clientId: 1, Now);
        cls.Enroll(clientId: 2, Now);

        cls.IsFull.Should().BeTrue();
    }

    [Fact]
    public void IsFull_BelowCapacity_ReturnsFalse()
    {
        var cls = CreateFutureClass(capacity: 5);
        cls.Enroll(clientId: 1, Now);

        cls.IsFull.Should().BeFalse();
    }

    [Fact]
    public void HasCapacityFor_EnoughSlots_ReturnsTrue()
    {
        var cls = CreateFutureClass(capacity: 5);
        cls.Enroll(clientId: 1, Now);

        cls.HasCapacityFor(3).Should().BeTrue();
    }

    [Fact]
    public void HasCapacityFor_NotEnoughSlots_ReturnsFalse()
    {
        var cls = CreateFutureClass(capacity: 2);
        cls.Enroll(clientId: 1, Now);

        cls.HasCapacityFor(3).Should().BeFalse();
    }

    [Fact]
    public void Reschedule_ValidFutureRange_UpdatesSchedule()
    {
        var cls = CreateFutureClass();
        var newStart = Now.AddDays(5);
        var newEnd = newStart.AddHours(3);
        var newRange = TimeRange.Create(newStart, newEnd);

        cls.Reschedule(newRange, Now);

        cls.Schedule.Should().Be(newRange);
    }

    [Fact]
    public void Reschedule_PastRange_ThrowsClassInPastError()
    {
        var cls = CreateFutureClass();
        var pastStart = Now.AddDays(-1);
        var pastEnd = pastStart.AddHours(2);
        var pastRange = TimeRange.Create(pastStart, pastEnd);

        var act = () => cls.Reschedule(pastRange, Now);
        act.Should().Throw<ClassInPastError>();
    }

    [Fact]
    public void TwoClasses_SameId_AreEqual()
    {
        var a = CreateFutureClass(id: 10);
        var b = CreateFutureClass(id: 10);
        a.Should().Be(b);
    }

    [Fact]
    public void TwoClasses_DifferentIds_AreNotEqual()
    {
        var a = CreateFutureClass(id: 1);
        var b = CreateFutureClass(id: 2);
        a.Should().NotBe(b);
    }
}
