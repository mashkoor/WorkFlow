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
    
    public partial class WFConditionMovement
    {
        public int Id { get; set; }
        public string ConditionGroup { get; set; }
        public int StepId { get; set; }
        public int CondnId { get; set; }
    
        public virtual WFMovement WFMovement { get; set; }
        public virtual WFCondition WFCondition { get; set; }
    }
}