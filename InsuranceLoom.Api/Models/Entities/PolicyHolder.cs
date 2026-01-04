namespace InsuranceLoom.Api.Models.Entities;

public class PolicyHolder
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? IdNumber { get; set; }
    public string? Phone { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Birthplace { get; set; }
    public string? Sex { get; set; }
    public string? CivilStatus { get; set; }
    public string? Occupation { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? MonthlyExpenses { get; set; }
    public string? EmploymentType { get; set; } // Employee, Self Employed
    public string? IncomeTaxNumber { get; set; }
    public DateTime? EmploymentStartDate { get; set; }
    public DateTime? EmploymentEndDate { get; set; }
    public string? AgencyName { get; set; }
    public string? AgencyContactNo { get; set; }
    public string? AgencyAddress { get; set; }
    public string? AgencyEmail { get; set; }
    public string? AgencySignatory { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

