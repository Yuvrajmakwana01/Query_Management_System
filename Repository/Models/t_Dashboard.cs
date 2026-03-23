using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class t_Dashboard
    {
        public int TotalQueries{get;set;}
   public int TotalTodayQueries{get;set;}
   public int PendingQueries{get;set;}
   public int PendingTodayQueries{get;set;}
   public int SolvedQuery{get;set;}
   public int SolvedTodayQueries{get;set;}
    }
}