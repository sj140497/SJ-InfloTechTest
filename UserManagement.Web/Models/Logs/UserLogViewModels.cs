using System;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.Web.Models.Logs;

public class UserLogListItemViewModel
{
    public long Id { get; set; }
    
    [Display(Name = "User")]
    public string UserName { get; set; } = string.Empty;
    
    public long UserId { get; set; }
    
    [Display(Name = "Action")]
    public string Action { get; set; } = string.Empty;
    
    [Display(Name = "Details")]
    public string Details { get; set; } = string.Empty;
    
    [Display(Name = "Timestamp")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm:ss}", ApplyFormatInEditMode = false)]
    public DateTime Timestamp { get; set; }
    
    [Display(Name = "Time Ago")]
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - Timestamp;
            
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            
            return "Just now";
        }
    }
}

public class UserLogListViewModel
{
    public List<UserLogListItemViewModel> Items { get; set; } = new();
    public string? FilterUserId { get; set; }
    public string? FilterAction { get; set; }
    public int TotalCount { get; set; }
}

public class UserLogDetailsViewModel
{
    public long Id { get; set; }
    
    [Display(Name = "User")]
    public string UserName { get; set; } = string.Empty;
    
    public long UserId { get; set; }
    
    [Display(Name = "Action Type")]
    public string Action { get; set; } = string.Empty;
    
    [Display(Name = "Details")]
    public string Details { get; set; } = string.Empty;
    
    [Display(Name = "Date & Time")]
    [DisplayFormat(DataFormatString = "{0:dddd, dd MMMM yyyy 'at' HH:mm:ss}", ApplyFormatInEditMode = false)]
    public DateTime Timestamp { get; set; }
    
    [Display(Name = "Time Ago")]
    public string TimeAgo
    {
        get
        {
            var timeSpan = DateTime.Now - Timestamp;
            
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays} day{((int)timeSpan.TotalDays != 1 ? "s" : "")} ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours} hour{((int)timeSpan.TotalHours != 1 ? "s" : "")} ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes} minute{((int)timeSpan.TotalMinutes != 1 ? "s" : "")} ago";
            
            return "Just now";
        }
    }
}
