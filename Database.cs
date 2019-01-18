using System;
using System.Collections.Generic;
using System.Linq;
using restapi.Models;

namespace restapi
{
    public static class Database
    {
        private static readonly IDictionary<string, Timecard> Timecards = 
            new Dictionary<string, Timecard>();
        
        public static IEnumerable<Timecard> All
        {
            get => Timecards.Values.ToList();
        }

        public static Timecard Find(string id)
        {
            Timecard timecard = null;

            if (Timecards.TryGetValue(id, out timecard) == true) 
            {
                return timecard;
            }
            else
            {
                return null;
            }
        }

        public static void Add(Timecard timecard)
        {
            Timecards.Add(timecard.Identity.Value, timecard);
        }
        public static void Delete(String id)
        {
            Timecards.Remove(id);
        }


    }

    /*
    * Role for employee/supervisor
   */
    public enum Roles
    {
        Employee = 0,
        Superviser = 1
    }

    /*
     * created some static resouce id 
     * maps from role Id to authenticate user
     * role id 0/1 is for employee/supervisor
     * 
    */
    public static class UserDB
    {

        private static readonly IDictionary<int, UserDetail> UserDetails =
             new Dictionary<int, UserDetail>()
            {
            { 1, new UserDetail {  Name = "tyu", Role= Roles.Employee, Id= 1 } },
            { 2, new UserDetail {  Name = "xyz", Role= Roles.Employee , Id= 2 } },
            { 3, new UserDetail {  Name = "abc", Role= Roles.Employee , Id= 3 } },
            { 4, new UserDetail {  Name = "def", Role= Roles.Superviser , Id= 4 } },
            { 5, new UserDetail {  Name = "ghj", Role= Roles.Superviser, Id= 5 } },
        };

        public static IEnumerable<UserDetail> All
        {
            get => UserDetails.Values.ToList();
        }

        public static UserDetail Find(int id)
        {
            UserDetail userDetail = null;

            if (UserDetails.TryGetValue(id, out userDetail) == true)
            {
                return userDetail;
            }
            else
            {
                return null;
            }
        }

    }

    /*
     * Id is resource Id
     */
    public class UserDetail
    {
        public string Name { get; set; }
        public Roles Role { get; set; }
        public int Id { get; set; }

    }
}