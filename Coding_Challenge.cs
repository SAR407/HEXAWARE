// LoanManagement
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace LoanManagementSystem
{
    //  Exception
    public class InvalidLoanException : Exception
    {
        public InvalidLoanException(string message) : base(message) { }
    }

    // Customer Class
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int CreditScore { get; set; }

        public Customer() { }

        public Customer(int id, string name, string email, string phone, string address, int score)
        {
            CustomerID = id; Name = name; EmailAddress = email;
            PhoneNumber = phone; Address = address; CreditScore = score;
        }
    }

    //  Loan class
    public class Loan
    {
        public int LoanId { get; set; }
        public Customer Customer { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int LoanTerm { get; set; } // in months
        public string LoanType { get; set; }
        public string LoanStatus { get; set; }

        public Loan() { }

        public Loan(int id, Customer customer, decimal amount, decimal rate, int term, string type, string status)
        {
            LoanId = id; Customer = customer; PrincipalAmount = amount;
            InterestRate = rate; LoanTerm = term; LoanType = type; LoanStatus = status;
        }
    }

    public class HomeLoan : Loan
    {
        public string PropertyAddress { get; set; }
        public int PropertyValue { get; set; }

        public HomeLoan() { }

        public HomeLoan(int id, Customer customer, decimal amount, decimal rate, int term, string status,
                         string address, int value)
            : base(id, customer, amount, rate, term, "HomeLoan", status)
        {
            PropertyAddress = address;
            PropertyValue = value;
        }
    }

    public class CarLoan : Loan
    {
        public string CarModel { get; set; }
        public int CarValue { get; set; }

        public CarLoan() { }

        public CarLoan(int id, Customer customer, decimal amount, decimal rate, int term, string status,
                       string model, int value)
            : base(id, customer, amount, rate, term, "CarLoan", status)
        {
            CarModel = model;
            CarValue = value;
        }
    }

    public interface ILoanRepository
    {
        void ApplyLoan(Loan loan);
        decimal CalculateInterest(int loanId);
        decimal CalculateInterest(decimal principal, decimal rate, int term);
        void LoanStatus(int loanId);
        decimal CalculateEMI(int loanId);
        decimal CalculateEMI(decimal principal, decimal rate, int term);
        void LoanRepayment(int loanId, decimal amount);
        void GetAllLoan();
        void GetLoanById(int loanId);
    }

    public class LoanRepositoryImpl : ILoanRepository
    {
        private List<Loan> loanDb = new List<Loan>();

        public void ApplyLoan(Loan loan)
        {
            Console.Write("Do you want to apply for this loan? (Yes/No): ");
            if (Console.ReadLine()?.ToLower() == "yes")
            {
                loan.LoanStatus = "Pending";
                loanDb.Add(loan);
                Console.WriteLine("Loan applied successfully.");
            }
        }

        public decimal CalculateInterest(int loanId)
        {
            var loan = loanDb.Find(l => l.LoanId == loanId);
            if (loan == null) throw new InvalidLoanException("Loan not found.");
            return CalculateInterest(loan.PrincipalAmount, loan.InterestRate, loan.LoanTerm);
        }

        public decimal CalculateInterest(decimal principal, decimal rate, int term)
        {
            return (principal * rate * term) / 1200;
        }

        public void LoanStatus(int loanId)
        {
            var loan = loanDb.Find(l => l.LoanId == loanId);
            if (loan == null) throw new InvalidLoanException("Loan not found.");

            loan.LoanStatus = loan.Customer.CreditScore > 650 ? "Approved" : "Rejected";
            Console.WriteLine($"Loan status updated: {loan.LoanStatus}");
        }

        public decimal CalculateEMI(int loanId)
        {
            var loan = loanDb.Find(l => l.LoanId == loanId);
            if (loan == null) throw new InvalidLoanException("Loan not found.");
            return CalculateEMI(loan.PrincipalAmount, loan.InterestRate, loan.LoanTerm);
        }

        public decimal CalculateEMI(decimal principal, decimal rate, int term)
        {
            decimal monthlyRate = rate / 12 / 100;
            return principal * monthlyRate * (decimal)Math.Pow(1 + (double)monthlyRate, term) /
                   ((decimal)Math.Pow(1 + (double)monthlyRate, term) - 1);
        }

        public void LoanRepayment(int loanId, decimal amount)
        {
            var emi = CalculateEMI(loanId);
            if (amount < emi)
            {
                Console.WriteLine("Amount is less than single EMI. Payment rejected.");
                return;
            }
            int paidEmis = (int)(amount / emi);
            Console.WriteLine($"You have paid {paidEmis} EMI(s).");
        }

        public void GetAllLoan()
        {
            foreach (var loan in loanDb)
                Console.WriteLine($"Loan ID: {loan.LoanId}, Type: {loan.LoanType}, Status: {loan.LoanStatus}");
        }

        public void GetLoanById(int loanId)
        {
            var loan = loanDb.Find(l => l.LoanId == loanId);
            if (loan == null) throw new InvalidLoanException("Loan not found.");
            Console.WriteLine($"Loan ID: {loan.LoanId}, Type: {loan.LoanType}, Customer: {loan.Customer.Name}, Status: {loan.LoanStatus}");
        }
    }

    public static class DBUtil
    {
        public static SqlConnection GetDBConn()
        {
            string connStr = "your_connection_string_here"; // Update this
            return new SqlConnection(connStr);
        }
    }

    class Program
    {
        static void Main()
        {
            ILoanRepository loanRepo = new LoanRepositoryImpl();

            while (true)
            {
                Console.WriteLine("\nMenu: 1. Apply Loan 2. Get All Loans 3. Get Loan 4. Repay Loan 5. Exit");
                Console.Write("Enter choice: ");
                int choice = int.Parse(Console.ReadLine());
                try
                {
                    switch (choice)
                    {
                        case 1:
                            Customer cust = new Customer(1, "Patty", "patty@example.com", "1234567890", "Chennai", 700);
                            HomeLoan hl = new HomeLoan(1001, cust, 500000, 7.5M, 120, "Pending", "Anna Nagar", 600000);
                            loanRepo.ApplyLoan(hl);
                            break;
                        case 2:
                            loanRepo.GetAllLoan();
                            break;
                        case 3:
                            Console.Write("Enter loan ID: ");
                            int lid = int.Parse(Console.ReadLine());
                            loanRepo.GetLoanById(lid);
                            break;
                        case 4:
                            Console.Write("Enter loan ID: ");
                            int repayId = int.Parse(Console.ReadLine());
                            Console.Write("Enter repayment amount: ");
                            decimal amount = decimal.Parse(Console.ReadLine());
                            loanRepo.LoanRepayment(repayId, amount);
                            break;
                        case 5:
                            return;
                        default:
                            Console.WriteLine("Invalid choice");
                            break;
                    }
                }
                catch (InvalidLoanException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected Error: {ex.Message}");
                }
            }
        }
    }
}
