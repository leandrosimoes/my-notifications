; (function () {
    var $area = $('#main'),
        types = {
            simple: 0,
            yes_or_no: 1,
            answer: 2
        },
        modes= {
            notification: 0,
            chat: 1
        };

    var Notification = function (id, title, message, type) {
        var _self = this;

        _self.id = id;
        _self.title = title;
        _self.message = message;
        _self.read = ko.observable(false);
        _self.readDescription = ko.pureComputed(function () {
            return !!_self.read() ? 'Yep' : 'Nope';
        });
        _self.type = ko.observable(type);
        _self.answer = ko.observable('');
        _self.showAnswer = ko.pureComputed(function () {
            return (_self.type() == types.yes_or_no || _self.type() == types.answer) && !!_self.answer();
        });
    };

    var Message = function (message, isAnswer) {
        var _self = this;

        _self.message = message;
        _self.isAnswer = !!isAnswer;
    };

    window.mn = {};
    window.mn.models = {};

    window.mn.koInit = function (args) {
        args.model = ko.dataFor(args.area[0]) || ko.mapping.fromJS(args.view)

        try {
            if (!!args.computeds && $.isFunction(args.computeds)) {
                args.computeds(args.model);
            }

            if (!!args.subscribers && $.isFunction(args.subscribers)) {
                args.subscribers(args.model);
            }

            ko.applyBindings(args.model, args.area[0]);
        } catch (e) {
            if (e.toString().indexOf('bindings multiple times') == -1) {
                console.error(e);
            }

            ko.mapping.fromJS(args.view, args.options || {}, args.model);
        }

        return args;
    };

    function setupSignalR(options) {
        var signalRModel = {};

        signalRModel._cnn = $.connection;
        signalRModel.myHub = signalRModel._cnn[options.hubName];

        if (!signalRModel.myHub) throw 'Hub not found!';

        for (var index in options.clientCallbacks) {
            signalRModel.myHub.client[options.clientCallbacks[index].name] = options.clientCallbacks[index].callback;
        }

        signalRModel._cnn.hub.url = options.hubUrl;

        signalRModel._cnn.hub.start()
            .done(options.onDone || function () { console.log('Start success!'); })
            .fail(options.onError || function (error) { console.log('Start error: ' + error); });

        return signalRModel;
    };

    var koNotif = window.mn.koInit({
        area: $area,
        view: {
            Title: '',
            Message: '',
            MessageChat: '',
            Block: true,
            Notifications: [],
            Users: [],
            SelectedUser: '',
            Errors: [],
            Types: ['Simple', 'Yes or No', 'Answer'],
            SelectedType: '',
            NotificationMode: false,
            ChatMode: true,
            ChatMessages: [],
            ChangeMode: function (mode) {
                window.mn.models.notificationsModel.NotificationMode(mode == modes.notification);
                window.mn.models.notificationsModel.ChatMode(mode == modes.chat);
            },
            SelectUser: function (id) {
                window.mn.models.notificationsModel.SelectedUser(id);
            },
            SendChatMessage: function(){
                window.mn.models.notificationsModel.Errors([]);

                var data = {
                    message: window.mn.models.notificationsModel.MessageChat(),
                    user: window.mn.models.notificationsModel.SelectedUser()
                };

                if (!data.message) {
                    window.mn.models.notificationsModel.Errors.push('The "Message" is required');
                }

                if (!data.user) {
                    window.mn.models.notificationsModel.Errors.push('You have to chose an "User" to send the message.');
                }

                if (window.mn.models.notificationsModel.Errors().length > 0) return;

                window.mn.models.notificationsModel.Block(true);

                $.ajax({
                    type: "POST",
                    url: "/Home/SendChatMessage",
                    contentType: "application/json charset=utf-8",
                    data: JSON.stringify(data),
                    dataType: "json",
                    success: function (response) {
                        if (response.Success) {
                            window.mn.models.notificationsModel.MessageChat('');

                            var newMessage = new Message(response.message, false);
                            window.mn.models.notificationsModel.ChatMessages.push(newMessage);
                        } else {
                            window.mn.models.notificationsModel.Errors.push(response.ErrorMessage);
                        }

                        window.mn.models.notificationsModel.Block(false);
                    }
                });
            },
            SendNotification: function () {
                window.mn.models.notificationsModel.Errors([]);

                var data = {
                    title: window.mn.models.notificationsModel.Title(),
                    message: window.mn.models.notificationsModel.Message(),
                    user: window.mn.models.notificationsModel.SelectedUser(),
                    type: types[window.mn.models.notificationsModel.SelectedType().toLowerCase().replace(/\s/g, '_')]
                };

                if (!data.title) {
                    window.mn.models.notificationsModel.Errors.push('The "Title" is required');
                }

                if (!data.message) {
                    window.mn.models.notificationsModel.Errors.push('The "Message" is required');
                }

                if (!data.user) {
                    window.mn.models.notificationsModel.Errors.push('You have to chose an "User" to send the notification.');
                }

                if (window.mn.models.notificationsModel.Errors().length > 0) return;

                window.mn.models.notificationsModel.Block(true);

                $.ajax({
                    type: "POST",
                    url: "/Home/SendNotification",
                    contentType: "application/json charset=utf-8",
                    data: JSON.stringify(data),
                    dataType: "json",
                    success: function (response) {
                        if (response.Success) {
                            window.mn.models.notificationsModel.Title('');
                            window.mn.models.notificationsModel.Message('');

                            var newNotification = new Notification(response.id, response.title, response.message, response.type);
                            window.mn.models.notificationsModel.Notifications.push(newNotification);
                        } else {
                            window.mn.models.notificationsModel.Errors.push(response.ErrorMessage);
                        }

                        window.mn.models.notificationsModel.Block(false);
                    }
                });
            }
        },
        computeds: function (model) {
            model.HasNotifications = ko.computed(function () {
                return model.Notifications().length > 0;
            });
            model.HasErrors = ko.computed(function () {
                return model.Errors().length > 0;
            });
        }
    });

    koNotif.model.signalRModel = setupSignalR({
        hubName: 'notificationsHub',
        hubUrl: 'http://localhost:51186/signalr',
        clientCallbacks: [
            {
                name: 'notificationRead',
                callback: function (id, title, message, answer) {
                    var existentNotif = window.mn.models.notificationsModel.Notifications();
                    for (var index in existentNotif) {
                        if (existentNotif[index].id === id) {
                            existentNotif[index].read(true);
                            existentNotif[index].answer(answer);
                        }
                    }
                }
            },
            {
                name: 'newClientOnline',
                callback: function (id) {
                    window.mn.models.notificationsModel.Users.push(id);
                }
            },
            {
                name: 'disconnectUser',
                callback: function (id) {
                    window.mn.models.notificationsModel.Users.remove(id);
                }
            },
            {
                name: 'chatMessageReceived',
                callback: function (idUser, message, isAnswer) {
                    var newMessage = new Message(message, isAnswer);
                    window.mn.models.notificationsModel.ChatMessages.push(newMessage);
                }
            }
        ],
        onDone: function () {
            window.mn.models.notificationsModel.Block(false);
        }
    });

    window.mn.models.notificationsModel = koNotif.model;
})();