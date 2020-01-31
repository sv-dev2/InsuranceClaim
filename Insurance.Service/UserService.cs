
using Insurance.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Insurance.Service
{
    public class UserService
    {
        public List<Domain.City> GetAllCity()
        {
            return InsuranceContext.Cities.All().ToList();
        }

        public static string ReplaceSpecialChracter(string text)
        {
            return Regex.Replace(text, @"[^0-9a-zA-Z]+", " ");
        }

        public Customer GetCustomerDetail(string userId)
        {
            return InsuranceContext.Customers.Single(where: $"UserID = '" + userId + "'");
        }

        public Customer GetLastCustomerDetail()
        {
            var customerDetail = InsuranceContext.Query("select top 1 * from Customer order by id desc").Select(x => new Customer()
            {
                //Id = x.Id,
                CustomerId= x.Id
            }).FirstOrDefault();

            return customerDetail;
        }

        public int SaveCustomer(Customer customerData)
        {
                  int customerId = 0;
            try
            {
                InsuranceContext.Customers.Insert(customerData);
                customerId = customerData.Id;
            }
            catch(Exception ex)
            {

            }
            return customerId;
        }

        public Customer GetCustomerDetailById(int? customerId)
        {
            return InsuranceContext.Customers.Single(customerId);
        }

        public void SaveSmsLog(SmsLog smsLog)
        {
            InsuranceContext.SmsLogs.Insert(smsLog);
        }




    }
}
