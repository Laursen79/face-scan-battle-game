using System.Collections.Generic;
using System.Linq;
using Battle.Laursen;
using UnityEngine;

namespace Battle
{
    public class CameraTargetManager : Singleton<CameraTargetManager>
    {
        private List<CameraTarget> _targets = new List<CameraTarget>();

        public void AddCameraTarget(CameraTarget target) => _targets.Add(target);

        /// Finds the midpoint between all targets
        public Vector3 Midpoint()
        {
            var temp = Vector3.zero;
            if (!_targets?.Any() ?? false)
                return temp;
            
            // ReSharper disable once PossibleNullReferenceException
            _targets.ForEach(delegate(CameraTarget target) { temp += target.transform.position; });
            return temp / _targets.Count;
        }

        public CameraTarget TargetFarthestFromMiddle()
        {
            var midpoint = Midpoint();
            CameraTarget temp = null;
            var previousRelativePosition = Vector3.zero;
            _targets.ForEach(delegate(CameraTarget target)
            {
                var relativePos = target.transform.position - midpoint;
                if(relativePos.sqrMagnitude > previousRelativePosition.sqrMagnitude) 
                    temp = target;
            });
            return temp;
        }

        /// Returns the CameraTarget that is farthest from point.
        /// <param name="point"> The point to compare against. </param>
        public CameraTarget TargetFarthestFromPoint(Vector3 point)
        {

            CameraTarget temp = null;
            var previousRelativePosition = Vector3.zero;
            _targets.ForEach(delegate(CameraTarget target)
            {
                var relativePos = target.transform.position - point;
                if(relativePos.sqrMagnitude > previousRelativePosition.sqrMagnitude) 
                    temp = target;
            });
            return temp;
        }

    }
}