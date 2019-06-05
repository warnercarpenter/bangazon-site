using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bangazon.Models.OrderViewModels
{
    public class OrderPayments
    {
        public Order Order { get; set; }
        public List<PaymentType> PaymentTypes { get; set; }
    }
}
