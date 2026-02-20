class ChatClient
{
    Connect()
    {
        this.dom_chat_log.innerHTML = "";

        this.websocket_client = new WebSocket(this.serverUrl);

        this.websocket_client.onmessage = (event) =>
        {
            let chat_message = JSON.parse(event.data);

            let date = new Date(chat_message.TimeUtc);

            // YYYY-MM-DD
            let date_year = date.getFullYear();

            let date_month = date.getMonth() + 1;
            if (date_month < 10) date_month = "0" + date_month;

            let date_day = date.getDate();
            if (date_day < 10) date_day = "0" + date_day;

            // HH-MM-SS
            let date_hour = date.getHours();
            if (date_hour < 10) date_hour = "0" + date_hour;
            let date_min  = date.getMinutes();
            if (date_min  < 10) date_min  = "0" + date_min;
            let date_sec  = date.getSeconds();
            if (date_sec  < 10) date_sec  = "0" + date_sec;

            let time_str =
                date_year + "-" + date_month + "-" + date_day + " " +
                date_hour + ":" + date_min + ":" + date_sec;

            let dom_chat_message = document.createElement('div');
            dom_chat_message.style = "display: flex; align-items: flex-start;";

            let dom_message_time = document.createElement('div');
            dom_message_time.style = "font-weight: bold; width: 156px; flex-shrink: 0;";
            dom_message_time.innerHTML = time_str;
            dom_chat_message.append(dom_message_time);

            let dom_message_text = document.createElement('div');
            dom_message_text.style = "flex: 1; white-space: pre; max-width: 100%";
            dom_message_text.innerHTML = chat_message.Text.replaceAll("\n", "<br/>");
            dom_chat_message.append(dom_message_text);

            this.dom_chat_log.append(dom_chat_message);
        }

        this.websocket_client.onclose = (event) =>
        {
            this.websocket_client.onopen    = null;
            this.websocket_client.onmessage = null;
            this.websocket_client.onerror   = null;
            this.websocket_client.onclose   = null;

            this.websocket_client = null;
            setTimeout(() => { this.Connect(); }, 10000);
        }
    }

    constructor(url)
    {
        this.dom_chat_log = document.getElementById("chat_log");

        this.serverUrl = url;
        this.Connect();

        this.dom_chat_input = document.getElementById("chat_input");
        this.dom_chat_input.addEventListener
        (
            "keydown",
            (event) => {
                if (event.ctrlKey && event.key == "Enter")
                    this.SendInput();
            }
        )
    }

    SendInput()
    {
        this.websocket_client.send(this.dom_chat_input.value);
        this.dom_chat_input.value = "";
    }
}
