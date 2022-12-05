namespace AppInfo
{
    public class UserInfoContainer
    {
        public string Id { get; set; }
        public string VkId { get; set; }
        public string Name { get; set; }
        public string AvatarPath { get; set; }

        public bool IsVk => string.IsNullOrWhiteSpace(VkId) == false;
    }
}