using Microsoft.AspNetCore.SignalR;

namespace SmartStorageBackend.Hubs
{
    public class DashboardHub : Hub
    {

        // Этот метод вызываеться при update робота в БД
        public async Task SendRobotUpdate(object robot)
        {
            await Clients.All.SendAsync("robot_update", robot);
        }

        // Этот метод вызываеться при малом кол-ве товаров на складе
        public async Task SendInventoryAlert(object alert)
        {
            await Clients.All.SendAsync("inventory_alert", alert);
        }
    }
}
