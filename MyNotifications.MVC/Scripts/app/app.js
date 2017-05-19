; (function () {
    var $area = $('#main'),
        types = {
            simple: 0,
            yes_or_no: 1,
            answer: 2
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

    window.koInit = function (args) {
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
        var signalR = {};

        signalR._cnn = $.connection;
        signalR.myHub = signalR._cnn[options.hubName];

        if (!signalR.myHub) throw 'Hub not found!';

        for (var index in options.clientCallbacks) {
            signalR.myHub.client[options.clientCallbacks[index].name] = options.clientCallbacks[index].callback;
        }

        for (var index in options.serverCallbacks) {
            signalR[options.serverCallbacks[index].name] = options.serverCallbacks[index].callback;
        }

        signalR._cnn.hub.url = options.hubUrl;

        signalR._cnn.hub.error(function (error) {
            window.notificationsModel.Errors.push(error);
        });
        signalR._cnn.hub.start()
            .done(options.onConnectionDone || function () { console.log('Start success!'); })
            .fail(options.onConnectionError || function (error) { console.log('Start error: ' + error); });

        return signalR;
    };

    var koNotif = window.koInit({
        area: $area,
        view: {
            Title: '',
            Message: '',
            Block: true,
            Notifications: [],
            Users: [],
            SelectedUser: '',
            Errors: [],
            Types: ['Simple', 'Yes or No', 'Answer'],
            SelectedType: '',
            SelectUser: function (id) {
                window.notificationsModel.SelectedUser(id);
            },
            SendNotification: function () {
                window.notificationsModel.Errors([]);

                var data = {
                    title: window.notificationsModel.Title(),
                    message: window.notificationsModel.Message(),
                    user: window.notificationsModel.SelectedUser(),
                    type: types[window.notificationsModel.SelectedType().toLowerCase().replace(/\s/g, '_')]
                };

                if (!data.title) {
                    window.notificationsModel.Errors.push('The "Title" is required');
                }

                if (!data.message) {
                    window.notificationsModel.Errors.push('The "Message" is required');
                }

                if (!data.user) {
                    window.notificationsModel.Errors.push('You have to chose an "User" to send the notification.');
                }

                if (window.notificationsModel.Errors().length > 0) return;

                window.notificationsModel.Block(true);

                window.notificationsModel.signalR.sendNotification(data.title, data.message, data.user, data.type);
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

    koNotif.model.signalR = setupSignalR({
        hubName: 'notificationsHub',
        hubUrl: 'http://localhost:51186/signalr',
        clientCallbacks: [
            {
                name: 'notificationRead',
                callback: function (id, title, message, answer) {
                    var existentNotif = window.notificationsModel.Notifications();
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
                    window.notificationsModel.Users.push(id);
                }
            },
            {
                name: 'disconnectUser',
                callback: function (id) {
                    window.notificationsModel.Users.remove(id);
                }
            }
        ],
        serverCallbacks: [{
            name: 'sendNotification',
            callback: function (title, message, user, type) {
                window.notificationsModel.signalR.myHub.server.sendNotification(title, message, user, type)
                    .done(function (result) {
                        if (!!result.Success) {
                            var newNotification = new Notification(result.id, result.title, result.message, result.type);
                            window.notificationsModel.Notifications.push(newNotification);
                        } else {
                            window.notificationsModel.Erros.push(error);
                        }
                    });

                window.notificationsModel.Block(false);
            }
        }],
        onConnectionDone: function () {
            window.notificationsModel.Block(false);
        },
        onConnectionError: function (error) {
            window.notificationsModel.Erros.push(error);
        }
    });

    window.notificationsModel = koNotif.model;
})();