namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    public class RenderResult {
        public enum RenderStatuses {
            None,
            Success,
            Error,
            CannotCommunicate,
            Qued
        }
        public string Result { get; set; }
        public RenderStatuses Status { get; set; }
    }
}
