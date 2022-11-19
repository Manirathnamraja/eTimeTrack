using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eTimeTrack.Models
{
    public interface IProjectComponent
    {
        bool IsClosed { get; set; }
        Project GetParentProject();
        ProjectPart GetParentProjectPart();
        ProjectGroup GetParentProjectGroup();
        bool IsActive();
        string GetName();
        string GetDescription();
        string GetId();
        string GetDisplayName();
    }
}
