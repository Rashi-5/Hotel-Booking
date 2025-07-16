using System;
using System.Collections.Generic;
using System.Linq;
using HotelBookingSystem.Models.Booking;
using HotelBookingSystem.Helper;

namespace HotelBookingSystem.Services
{
    public class RoomService
    {
        private static readonly Lazy<RoomService> _instance = new Lazy<RoomService>(() => new RoomService());
        public static RoomService Instance => _instance.Value;

        private readonly List<RoomCardViewModel> _rooms;
        private readonly object _lock = new object();

        private RoomService()
        {
            // Initialize with default rooms
            _rooms = RoomDataHelper.GetDefaultRooms();
        }

        public List<RoomCardViewModel> GetAllRooms()
        {
            lock (_lock)
            {
                return _rooms.ToList();
            }
        }

        public RoomCardViewModel GetRoomByName(string roomName)
        {
            lock (_lock)
            {
                return _rooms.FirstOrDefault(r => r.RoomName == roomName);
            }
        }

        public void AddRoom(RoomCardViewModel room)
        {
            lock (_lock)
            {
                _rooms.Add(room);
            }
        }

        public void UpdateRoom(RoomCardViewModel updatedRoom)
        {
            lock (_lock)
            {
                var index = _rooms.FindIndex(r => r.RoomName == updatedRoom.RoomName);
                if (index >= 0)
                {
                    _rooms[index] = updatedRoom;
                }
            }
        }

        public void DeleteRoom(string roomName)
        {
            lock (_lock)
            {
                var room = _rooms.FirstOrDefault(r => r.RoomName == roomName);
                if (room != null)
                {
                    _rooms.Remove(room);
                }
            }
        }
    }
}
