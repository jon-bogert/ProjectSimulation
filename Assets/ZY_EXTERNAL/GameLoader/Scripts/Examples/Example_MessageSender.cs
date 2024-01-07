using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Example_MessageSender : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField = null;
    [SerializeField] private Button _sendMessageButton = null;
    [SerializeField] private TextMeshProUGUI _textMessageReceived = null;

    public void Initialize()
    {
        if(_sendMessageButton == null || _inputField == null)
        {
            Debug.LogError("Required components not assigned in inspector");
            return;
        }

        _sendMessageButton.onClick.AddListener(() => {
            Debug.Log("Sending Event Bus Message");
            OnSendMessageButtonClicked(_inputField.text); 
        });

        // If you want to subscribe to an event raised when this message is handled you can do it like this
        ServiceLocator.Get<EventBusCallbacks>().SimpleTextMessageHandled += OnSimpleTextMessageHandled;
    }

    public void OnSendMessageButtonClicked(string message)
    {
        var messenger = ServiceLocator.Get<IEventBusSystemHub>();
        messenger.Publish(new SimpleTextMessage(this, message));
    }

    private void OnSimpleTextMessageHandled(SimpleTextMessage stm)
    {
        if(_textMessageReceived != null)
        {
            _textMessageReceived.text = stm.Message;
        }
    }
}
