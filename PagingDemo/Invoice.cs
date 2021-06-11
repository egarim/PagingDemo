using DevExpress.Xpo;
using System;

namespace PagingDemo
{
    public class Invoice : XPObject
    {
        public Invoice() : base()
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public Invoice(Session session) : base(session)
        {
            // This constructor is used when an object is loaded from a persistent storage.
            // Do not place any code here.
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place here your initialization code.
        }

        Customer customer;
        decimal total;
        DateTime date;
        string invoiceNumber;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string InvoiceNumber
        {
            get => invoiceNumber;
            set => SetPropertyValue(nameof(InvoiceNumber), ref invoiceNumber, value);
        }

        public DateTime Date
        {
            get => date;
            set => SetPropertyValue(nameof(Date), ref date, value);
        }

        public decimal Total
        {
            get => total;
            set => SetPropertyValue(nameof(Total), ref total, value);
        }
        
        public Customer Customer
        {
            get => customer;
            set => SetPropertyValue(nameof(Customer), ref customer, value);
        }
    }

}