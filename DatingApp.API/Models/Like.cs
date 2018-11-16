namespace DatingApp.API.Models
{
    public class Like
    {
        public int LikerId { get; set; }    // id if the user that likes someone
        public int LikeeId { get; set; }    // id of the user wh ois being liked
        public User Liker { get; set; }
        public User Likee { get; set; }
    }
}