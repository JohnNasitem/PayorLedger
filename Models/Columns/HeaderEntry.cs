//***********************************************************************************
//Program: HeaderEntry.cs
//Description: Entry in the Header database table
//Date: May 5, 2025
//Author: John Nasitem
//***********************************************************************************

using PayorLedger.Services.Database;

namespace PayorLedger.Models.Columns
{
    public class HeaderEntry : IColumn, IDatabaseAction
    {
        private static int _tempIdIncr = -1;



        /// <summary>
        /// Id of the header
        /// </summary>
        public long Id { get; set; }



        /// <summary>
        /// Name of the header
        /// </summary>
        public string Name { get; set; }



        /// <summary>
        /// Order the header should be displayed in
        /// </summary>
        public int Order { get; set; }



        /// <summary>
        /// Subheaders under this header
        /// </summary>
        public List<SubheaderEntry> Subheaders { get; set; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public HeaderEntry(long headerId, string headerName, int headerOrder, ChangeState state)
        {
            if (headerId == -1)
            {
                Id = _tempIdIncr;
                _tempIdIncr--;
            }
            else
                Id = headerId;

            Name = headerName;
            Order = headerOrder;
            Subheaders = [];
            State = state;
        }



        public HeaderEntry(HeaderEntry otherInstance)
        {
            Id = otherInstance.Id;
            Name = otherInstance.Name;
            Order = otherInstance.Order;
            Subheaders = new List<SubheaderEntry>(otherInstance.Subheaders);
            State = otherInstance.State;
        }
    }
}
