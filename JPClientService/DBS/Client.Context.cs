﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JPClientService.DBS
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class ClientEntities : DbContext
    {
        public ClientEntities()
            : base("name=ClientEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
    
        public virtual ObjectResult<string> WFDefs(string str, string opration)
        {
            var strParameter = str != null ?
                new ObjectParameter("str", str) :
                new ObjectParameter("str", typeof(string));
    
            var oprationParameter = opration != null ?
                new ObjectParameter("opration", opration) :
                new ObjectParameter("opration", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<string>("WFDefs", strParameter, oprationParameter);
        }
    }
}
