using System;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace CurrencyBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static ReceiverOptions _receiverOptions;

        static async Task Main(string[] args)
        {
            _botClient = new TelegramBotClient("7899452442:AAGXDPVvaFLhOf6Vjdv-cZqIQPptDd14e8g");
            _receiverOptions = new ReceiverOptions 
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
                ThrowPendingUpdates = true,
            };

            using var cts = new CancellationTokenSource();
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); 

            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1);
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message)
                {
                    Chat chat = update.Message.Chat;
                    switch (update.Message.Text)
                    {
                        case "/start":
                            await Start(botClient, chat);
                            return;
                    }
                }

                if(update.Type == UpdateType.CallbackQuery)
                {
                    var callbackQuery = update.CallbackQuery;
                    var chat = callbackQuery.Message.Chat;
                    switch (callbackQuery.Data)
                    {
                        case "buttonUsd":
                            await GetUsd(botClient, callbackQuery, chat);
                            return;
                        case "buttonEur":
                            await GetEur(botClient, callbackQuery, chat);
                            return;
                        case "buttonCny":
                            await GetCny(botClient, callbackQuery, chat);
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task GetCny(ITelegramBotClient botClient, CallbackQuery callbackQuery, Chat chat)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "А это полноэкранный текст!", showAlert: true);

            await botClient.SendTextMessageAsync(
                chat.Id,
                $"15");
        }

        private static async Task GetEur(ITelegramBotClient botClient, CallbackQuery callbackQuery, Chat chat)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Тут может быть ваш текст!");
            await botClient.SendTextMessageAsync(chat.Id,$"110");
        }

        private static async Task GetUsd(ITelegramBotClient botClient, CallbackQuery callbackQuery, Chat chat)
        {
            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await botClient.SendTextMessageAsync(chat.Id,$"90");
        }

        private static async Task Start(ITelegramBotClient botClient, Chat chat)
        {
            InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(
                                                new List<InlineKeyboardButton[]>()
                                                {
                                                    new InlineKeyboardButton[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData("USD", "buttonUsd"),
                                                    },
                                                    new InlineKeyboardButton[]
                                                    {
                                                        InlineKeyboardButton.WithCallbackData("EUR", "buttonEur"),
                                                        InlineKeyboardButton.WithCallbackData("CNY", "buttonCny"),
                                                    },
                                                });

            await botClient.SendTextMessageAsync(chat.Id,"Выбери валюту:",replyMarkup: inlineKeyboard); 
        }

        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}
