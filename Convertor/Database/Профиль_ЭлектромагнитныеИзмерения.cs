//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Convertor.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Профиль_ЭлектромагнитныеИзмерения
    {
        public int id { get; set; }
        public Nullable<int> id_профиля { get; set; }
        public Nullable<int> id_эл_измерений { get; set; }
        public Nullable<System.DateTime> дата_добавления_записи { get; set; }
        public Nullable<System.DateTime> дата_последнего_изменения_записи { get; set; }
        public Nullable<bool> удален { get; set; }
    
        public virtual Профиль Профиль { get; set; }
        public virtual Электромагнитные_измерения Электромагнитные_измерения { get; set; }
    }
}
