using EeD_BE_EeD.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace EeD_BE_EeD.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        // ===== بطاقات الإحصاءات (Cards) =====
        public int TotalUsers { get; set; }
        public int ActiveServices { get; set; }
        public int PendingExchanges { get; set; }
        public int NewReviews { get; set; }

        // ===== الرسوم البيانية (Charts) =====
        public List<MonthlyCount> NewUsersPerMonth { get; set; } = new();
        public List<CategoryCount> ActiveServicesPerCategory { get; set; } = new();

        // ===== آخر الأنشطة (Recent Activities) =====
        public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
        public string CurrentUserFullName { get; set; } = "";

    }

    public class MonthlyCount
    {
        public string Month { get; set; } = default!; // e.g. "2025-01"
        public int Count { get; set; }
    }

    public class CategoryCount
    {
        public string Category { get; set; } = default!;
        public int Count { get; set; }
    }

    // ✅ هذا الموديل الصغير مخصص لعنصر واحد في “Recent Activities”
    public class ActivityItemViewModel
    {
        public string Action { get; set; }       // Register / CreateService / ExchangeCompleted / ReviewAdded ...
        public string Description { get; set; }  // النصّ اللي رح ينعرض
        public DateTime CreatedAt { get; set; }  // وقت الحدث (UTC)

        // 🕓 وقت ودّي للعرض (مثل "2 hours ago")
        public string TimeAgo
        {
            get
            {
                var diff = DateTime.UtcNow - CreatedAt;
                if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
                if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
                return $"{(int)diff.TotalDays} days ago";
            }
        }

        // 🎨 مفتاح أيقونة (CSS class) حسب نوع الحدث
        public string IconKey =>
            Action switch
            {
                "Register" => "icon-user",
                "CreateService" => "icon-service",
                "ExchangeCompleted" => "icon-exchange",
                "ReviewAdded" => "icon-review",
                _ => "icon-default"
            };

        // 🧩 SVG أيقونة ديناميكية حسب نوع الـAction
        public string IconSvg
        {
            get
            {
                return Action switch
                {
                    "Register" => @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 256 256"" fill=""currentColor"">
									<path d=""M230.92,212c-15.23-26.33-38.7-45.21-66.09-54.16a72,72,0,1,0-73.66,0C63.78,166.78,40.31,185.66,25.08,212a8,8,0,1,0,13.85,8c18.84-32.56,52.14-52,89.07-52s70.23,19.44,89.07,52a8,8,0,1,0,13.85-8ZM72,96a56,56,0,1,1,56,56A56.06,56.06,0,0,1,72,96Z""></path>
								</svg>",

                    "CreateService" => @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 256 256"" fill=""currentColor"">
									<path d=""M216,56H176V48a24,24,0,0,0-24-24H104A24,24,0,0,0,80,48v8H40A16,16,0,0,0,24,72V200a16,16,0,0,0,16,16H216a16,16,0,0,0,16-16V72A16,16,0,0,0,216,56ZM96,48a8,8,0,0,1,8-8h48a8,8,0,0,1,8,8v8H96ZM216,72v41.61A184,184,0,0,1,128,136a184.07,184.07,0,0,1-88-22.38V72Zm0,128H40V131.64A200.19,200.19,0,0,0,128,152a200.25,200.25,0,0,0,88-20.37V200ZM104,112a8,8,0,0,1,8-8h32a8,8,0,0,1,0,16H112A8,8,0,0,1,104,112Z""></path>
								</svg>",

                    "ExchangeCompleted" => @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 256 256"" fill=""currentColor"">
									<path d=""M213.66,181.66l-32,32a8,8,0,0,1-11.32-11.32L188.69,184H48a8,8,0,0,1,0-16H188.69l-18.35-18.34a8,8,0,0,1,11.32-11.32l32,32A8,8,0,0,1,213.66,181.66Zm-139.32-64a8,8,0,0,0,11.32-11.32L67.31,88H208a8,8,0,0,0,0-16H67.31L85.66,53.66A8,8,0,0,0,74.34,42.34l-32,32a8,8,0,0,0,0,11.32Z""></path>
								</svg>",

                    "ReviewAdded" => @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 256 256"" fill=""currentColor"">
									<path d=""M239.2,97.29a16,16,0,0,0-13.81-11L166,81.17,142.72,25.81h0a15.95,15.95,0,0,0-29.44,0L90.07,81.17,30.61,86.32a16,16,0,0,0-9.11,28.06L66.61,153.8,53.09,212.34a16,16,0,0,0,23.84,17.34l51-31,51.11,31a16,16,0,0,0,23.84-17.34l-13.51-58.6,45.1-39.36A16,16,0,0,0,239.2,97.29Zm-15.22,5-45.1,39.36a16,16,0,0,0-5.08,15.71L187.35,216v0l-51.07-31a15.9,15.9,0,0,0-16.54,0l-51,31h0L82.2,157.4a16,16,0,0,0-5.08-15.71L32,102.35a.37.37,0,0,1,0-.09l59.44-5.14a16,16,0,0,0,13.35-9.75L128,32.08l23.2,55.29a16,16,0,0,0,13.35,9.75L224,102.26S224,102.32,224,102.33Z""></path>
								</svg>",
                    "Login" => @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20""
                          viewBox=""0 0 256 256"" fill=""currentColor"" aria-hidden=""true"">
                       <path d=""M136,216H64a32,32,0,0,1-32-32V72A32,32,0,0,1,64,40h72a8,8,0,0,1,0,16H64A16,16,0,0,0,48,72V184a16,16,0,0,0,16,16h72a8,8,0,0,1,0,16Zm79.51-96.49-40-40a8,8,0,0,0-11,11l25.51,25.52H112a8,8,0,0,0,0,16h78.05l-25.51,25.52a8,8,0,0,0,11,11l40-40A8,8,0,0,0,215.51,119.51Z""/>
                     </svg>

                                            ",

                    _ => @"<svg xmlns='http://www.w3.org/2000/svg' width='20' height='20' viewBox='0 0 256 256' fill='currentColor'>
                                <path d='M128,24A104,104,0,1,0,232,128,104.12,104.12,0,0,0,128,24ZM128,208a80,80,0,1,1,80-80A80.09,80.09,0,0,1,128,208Z'/>
                           </svg>"
                };
            }
        }
    }
}
