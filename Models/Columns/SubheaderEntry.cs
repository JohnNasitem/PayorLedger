//***********************************************************************************
//Program: SubheaderEntry.cs
//Description: Entry in the Subheader database table
//Date: May 5, 2025
//Author: John Nasitem
//***********************************************************************************

using PayorLedger.Services.Database;

namespace PayorLedger.Models.Columns
{
    public class SubheaderEntry : IColumn, IDatabaseAction
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
        /// Order the subheader should be displayed in
        /// </summary>
        public int Order { get; set; }


        /// <summary>
        /// Header this subheader belongs to
        /// </summary>
        public HeaderEntry Header { get; set; }



        /// <summary>
        /// State of the object in relation to the database
        /// </summary>
        public ChangeState State { get; set; }



        public SubheaderEntry(long subheaderId, string subheaderName, int subheaderOrder, HeaderEntry header, ChangeState state)
        {
            if (subheaderId == -1)
            {
                Id = _tempIdIncr;
                _tempIdIncr--;
            }
            else
                Id = subheaderId;
            Name = subheaderName;
            Order = subheaderOrder;
            Header = header;
            State = state;
        }



        public SubheaderEntry(SubheaderEntry otherInstance)
        {
            Id = otherInstance.Id;
            Name = otherInstance.Name;
            Order = otherInstance.Order;
            Header = otherInstance.Header;
            State = otherInstance.State;
        }
    }
}
