//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JPWFUI
{
    using System;
    using System.Collections.Generic;
    
    public partial class WFMovement
    {
        public WFMovement()
        {
            this.WFConditionMovements = new HashSet<WFConditionMovement>();
        }
    
        public int Id { get; set; }
        public Nullable<int> WFId { get; set; }
        public int StatusId { get; set; }
        public string ToStatus { get; set; }
        public int ToStatusId { get; set; }
        public string Description { get; set; }
    
        public virtual ICollection<WFConditionMovement> WFConditionMovements { get; set; }
    }
}
