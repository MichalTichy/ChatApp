var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.on("ClientConnected", function (username) {
    dotvvm.viewModels.root.viewModel.AvailableUsers().forEach((item) => {
        var userInfo = item();
        if (userInfo.UserName() === username) {
            userInfo.Online(true);
        }
    });
    dotvvm.viewModels.root.viewModel.ChatRooms().forEach((item) => {
        var chatRoom = item();
        if (chatRoom.Name() === username) {
            chatRoom.IsOnline(true);
        }
    });
});
connection.on("ClientDisconnected", function (username) {

    dotvvm.viewModels.root.viewModel.ChatRooms().forEach((item) => {
        var chatRoom = item();
        if (chatRoom.Name()===username) {
            chatRoom.IsOnline(false);
        }
    });

    dotvvm.viewModels.root.viewModel.AvailableUsers().forEach((item) => {
        var userInfo = item();
        if (userInfo.UserName() === username) {
            userInfo.Online(false);
        }
    });
});

connection.on("ReceiveMessage", function (chatRoomId, data, date, sender, senderName, isGroupMessage) {
    var activeChatRoom = dotvvm.viewModels.root.viewModel.ActiveChatRoom();
    if (activeChatRoom.Id() === chatRoomId) {
        var message = {};
        message.Data = ko.observable(data);
        message.Date = ko.observable(date);
        message.Sender = ko.observable(sender);
        message.SenderName = ko.observable(senderName);
        message.Recipient = ko.observable(chatRoomId);
        message.IsGroupMessage = ko.observable(isGroupMessage);
        message.Id = ko.observable("000000");
        activeChatRoom.Messages().Items.push(ko.observable(message));
    }
});
connection.start();