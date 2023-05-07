using UnityEngine;

namespace TunicArchipelago
{
    internal class ArchipelagoBehavior : MonoBehaviour
    {
        public static ArchipelagoBehavior instance { get; set; }

        private ArchipelagoIntegration integration;

        public void Start()
        {
            this.integration = new ArchipelagoIntegration();
        }

        public void Update()
        {
            this.integration.Update();
        }

        public void OnDestroy()
        {
            this.integration.Disconnect();
        }

        public void Connect()
        {
            this.integration.Connect();
        }

        public void Disconnect()
        {
            this.integration.Disconnect();
        }

        public ArchipelagoIntegration.CheckState GetCheckState(GameObject gameObject)
        {
            return this.integration.GetCheckState(gameObject);
        }

        public void ActivateCheck(GameObject gameObject)
        {
            this.integration.ActivateCheck(gameObject);
        }
    }
}
