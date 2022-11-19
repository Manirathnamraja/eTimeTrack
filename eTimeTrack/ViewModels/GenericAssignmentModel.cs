using System.Collections.Generic;

namespace eTimeTrack.ViewModels
{
    public class GenericAssignmentModel<T, TU, TV>
    {
        public T AssignmentRecipient { get; set; }
        public bool IsAdmin { get; set; }
        public List<TU> AvailableList { get; set; }
        public List<TV> AssignedList { get; set; }
    }
}