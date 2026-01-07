using Intervu.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intervu.Application.Services
{
    public class InterviewRoomCache
    {
        private readonly Dictionary<Guid, InterviewRoom> _rooms = new();

        public IReadOnlyCollection<InterviewRoom> Rooms => _rooms.Values;

        public void SetAll(IEnumerable<InterviewRoom> rooms)
        {
            _rooms.Clear();
            foreach (var room in rooms)
                _rooms[room.Id] = room;
        }

        public void Add(InterviewRoom room)
        {
            _rooms[room.Id] = room;
        }

        public void Update(InterviewRoom room)
        {
            _rooms[room.Id] = room;
        }

        public void Remove(Guid id)
        {
            _rooms.Remove(id);
        }
    }
}
