using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AjaxChat.Models;

namespace AjaxChat.Controllers
{
    public class HomeController : Controller
    {
        static ChatModel chatModel;

        public ActionResult Index(string user, bool? logOn, bool? logOff, string chatMessage)
        {
            try
            {
                if (chatModel == null) chatModel = new ChatModel();

                //оставляем только последние 90 сообщений
                if (chatModel.Messages.Count > 100)
                    chatModel.Messages.RemoveRange(0, 90);

                // если обычный запрос, просто возвращаем представление
                if (!Request.IsAjaxRequest())
                {
                    return View(chatModel);
                }
                else if (logOn != null && (bool)logOn)
                {
                    //проверяем, существует ли уже такой пользователь
                    if (chatModel.Users.FirstOrDefault(u => u.Name == user) != null)
                    {
                        throw new Exception("Користувача з таким ніком не існує");
                    }
                    else if (chatModel.Users.Count > 10)
                    {
                        throw new Exception("Чат заповнений");
                    }
                    else
                    {
                        // добавляем в список нового пользователя
                        chatModel.Users.Add(new ChatUser()
                        {
                            Name = user,
                            LoginTime = DateTime.Now,
                            LastPing = DateTime.Now
                        });

                        // добавляем в список ссообщений сообщение о новом пользователе
                        chatModel.Messages.Add(new ChatMessage()
                        {
                            Text = user +" увійшов в чат",
                            Date = DateTime.Now
                        });
                    }

                    return PartialView("ChatRoom", chatModel);
                }
                else if (logOff != null && (bool)logOff)
                {
                    LogOff(chatModel.Users.FirstOrDefault(u => u.Name == user));
                    return PartialView("ChatRoom", chatModel);
                }
                else
                {
                    ChatUser currentUser = chatModel.Users.FirstOrDefault(u => u.Name == user);

                    //для каждлого пользователя запоминаем воемя последнего обновления
                    currentUser.LastPing = DateTime.Now;

                    // удаляем неаквтивных пользователей
                    List<ChatUser> removeThese = new List<ChatUser>();
                    foreach (Models.ChatUser usr in chatModel.Users)
                    {
                        TimeSpan span = DateTime.Now - usr.LastPing;
                        if (span.TotalSeconds > 15)
                            removeThese.Add(usr);
                    }
                    foreach (ChatUser u in removeThese)
                    {
                        LogOff(u);
                    }

                    // додаємо в список нове повідомлення
                    if (!string.IsNullOrEmpty(chatMessage))
                    {
                        chatModel.Messages.Add(new ChatMessage()
                        {
                            User = currentUser,
                            Text = chatMessage,
                            Date = DateTime.Now
                        });
                    }
                    return PartialView("History", chatModel);
                }
            }
            catch (Exception ex)
            {
                //якщо помилка – відправляємо код 500
                Response.StatusCode = 500;
                return Content(ex.Message);
            }
        }
        // при виході користувача з чату, видаляємо його із списку
        public void LogOff(ChatUser user)
        {
            chatModel.Users.Remove(user);
            chatModel.Messages.Add(new ChatMessage()
            {
                Text = user.Name + " покинув чат.",
                Date = DateTime.Now
            });
        }
    }
}