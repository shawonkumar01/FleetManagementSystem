using Microsoft.AspNetCore.SignalR;

namespace FleetManagementSystem.Hubs
{
    public class DashboardHub : Hub
    {
        // Method for clients to join the dashboard group
        public async Task JoinDashboard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "DashboardUsers");
        }

        // Method for clients to leave the dashboard group
        public async Task LeaveDashboard()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "DashboardUsers");
        }

        // Server-side method to broadcast stats updates
        public static async Task BroadcastStatsUpdate(IHubContext<DashboardHub> hubContext, 
            int totalVehicles, int activeVehicles, int totalDrivers, int totalTrips)
        {
            await hubContext.Clients.Group("DashboardUsers").SendAsync("StatsUpdate", new
            {
                TotalVehicles = totalVehicles,
                ActiveVehicles = activeVehicles,
                TotalDrivers = totalDrivers,
                TotalTrips = totalTrips
            });
        }

        // Server-side method to broadcast new trip notification
        public static async Task BroadcastNewTrip(IHubContext<DashboardHub> hubContext, 
            string vehicleName, string driverName, string origin, string destination)
        {
            await hubContext.Clients.Group("DashboardUsers").SendAsync("NewTrip", new
            {
                VehicleName = vehicleName,
                DriverName = driverName,
                Origin = origin,
                Destination = destination,
                Timestamp = DateTime.UtcNow
            });
        }

        // Server-side method to broadcast maintenance alert
        public static async Task BroadcastMaintenanceAlert(IHubContext<DashboardHub> hubContext,
            string vehicleName, string serviceType, DateTime serviceDate)
        {
            await hubContext.Clients.Group("DashboardUsers").SendAsync("MaintenanceAlert", new
            {
                VehicleName = vehicleName,
                ServiceType = serviceType,
                ServiceDate = serviceDate
            });
        }

        // Server-side method to broadcast fuel record update
        public static async Task BroadcastFuelUpdate(IHubContext<DashboardHub> hubContext,
            string vehicleName, double liters, decimal cost)
        {
            await hubContext.Clients.Group("DashboardUsers").SendAsync("FuelUpdate", new
            {
                VehicleName = vehicleName,
                Liters = liters,
                Cost = cost,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
