using System;
namespace ROIDForumServer
{
    public class ServerMessages
    {
        public enum Controller
        {
            Chat = 0,
            Login = 1,
            Section = 2
        }

        public enum ChatMsg
        {
            Msg = 0,
            OnlineList = 1
        }

        public enum SectionMsg {
            AllThreads = 0,
            AddThread = 1,
            RemoveThread = 2,
            UpdateThread = 3,
            MoveThreadToTop = 4,
            AddComment = 5,
            RemoveComment = 6,
            UpdateComment = 7
        }

        public enum LoginMsg {
            GetAvatar = 0,
            LoginFailed = 1,
            LoggedOut = 2,
            RegisterFailed = 3,
            LoggedIn = 4,
        }

        public static byte[] LoggedInMessage(String username, String password)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Login);
            message.AddUint8((byte)LoginMsg.LoggedIn);
            message.AddString(username);
            message.AddString(password);

            return message.ToBuffer();
        }

        public static byte[] RegisterFailedMessage()
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Login);
            message.AddUint8((byte)LoginMsg.RegisterFailed);
            return message.ToBuffer();
        }

        public static byte[] LoggedOutMessage()
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Login);
            message.AddUint8((byte)LoginMsg.LoggedOut);
            return message.ToBuffer();
        }

        public static byte[] LoginFailedMessage()
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Login);
            message.AddUint8((byte)LoginMsg.LoginFailed);
            return message.ToBuffer();
        }

        public static byte[] GetAvatarMessage(String avatarURL)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Login);
            message.AddUint8((byte)LoginMsg.GetAvatar);
            message.AddString(avatarURL);
            return message.ToBuffer();
        }

        public static byte[] AllThreadsMessage(SectionController controller)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.AllThreads);
            message.AddString(controller.name);
            message.AddUint32((uint)controller.threadController.threads.Count);
            for (int i = 0; i < controller.threadController.threads.Count; i++)
            {
                ThreadInfo t = controller.threadController.threads[i];
                message.AddBinary(t.toBinary());
            }
            return message.ToBuffer();
        }

        public static byte[] AddThreadMessage(SectionController controller, ThreadInfo thread) {

            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.AddThread);
            message.AddString(controller.name);
            message.AddBinary(thread.toBinary());
            return message.ToBuffer();
        }

        public static byte[] RemoveThreadMessage(SectionController controller, ThreadInfo thread)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.RemoveThread);
            message.AddString(controller.name);
            message.AddUint32((uint)thread.id);
            return message.ToBuffer();
        }

        public static byte[] UpdateThreadMessage(SectionController controller, ThreadInfo thread)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.UpdateThread);
            message.AddString(controller.name);
            message.AddUint32((uint)thread.id);
            message.AddString(thread.title);
            message.AddString(thread.description);
            return message.ToBuffer();
        }

        public static byte[] MoveToTopThreadMessage(SectionController controller, ThreadInfo thread)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.MoveThreadToTop);
            message.AddString(controller.name);
            message.AddUint32((uint)thread.id);
            return message.ToBuffer();
        }

        public static byte[] AddCommentMessage(SectionController controller, CommentInfo comment)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.AddComment);
            message.AddString(controller.name);
            message.AddBinary(comment.toBinary());
            return message.ToBuffer();
        }

        public static byte[] RemoveCommentMessage(SectionController controller, CommentInfo comment)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.RemoveComment);
            message.AddString(controller.name);
            message.AddUint32((uint)comment.threadID);
            message.AddUint32((uint)comment.commentID);
            return message.ToBuffer();
        }

        public static byte[] UpdateCommentMessage(SectionController controller, CommentInfo comment)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Section);
            message.AddUint8((byte)SectionMsg.UpdateComment);
            message.AddString(controller.name);
            message.AddUint32((uint)comment.commentID);
            message.AddUint32((uint)comment.threadID);
            message.AddString(comment.comment);
            return message.ToBuffer();
        }
        
        public static byte[] ChatMessage(String chat) {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Chat);
            message.AddUint8((byte)ChatMsg.Msg);
            message.AddString(chat);
            return message.ToBuffer();
        }

        public static byte[] ChatOnlineList(String list)
        {
            var message = new MessageWriter();
            message.AddUint8((byte)Controller.Chat);
            message.AddUint8((byte)ChatMsg.OnlineList);
            message.AddString(list);
            return message.ToBuffer();
        }
    }
}
