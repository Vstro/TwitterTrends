using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TwitterTrends.Entities;

namespace TwitterTrends.Services
{
    public static class MapService
    {
        public static List<StateDrawModel> GetStatesDrawModels()
        {
            var statesDrawModels = new List<StateDrawModel>();
            foreach (var state in StateService.GetStatesCoordinates())
            {
                foreach (var polygon in state.Value[0])
                {

                }
            }
            StateService.GetStatesCoordinates();

        }
    }
}
