using Microsoft.AspNetCore.SignalR;

namespace SmartStorageBackend.Hubs
{
    public class DashboardHub : Hub
    {

        // ���� ����� ����������� ��� update ������ � ��
        public async Task SendRobotUpdate(object robot)
        {
            await Clients.All.SendAsync("robot_update", robot);
        }

        // ���� ����� ����������� ��� ����� ���-�� ������� �� ������
        public async Task SendInventoryAlert(object alert)
        {
            await Clients.All.SendAsync("inventory_alert", alert);
        }
    }
}
