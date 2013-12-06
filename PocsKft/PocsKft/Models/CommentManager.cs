using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PocsKft.Models
{
    public class CommentManager
    {
        private static volatile CommentManager instance;
        private static object syncRoot = new Object();
        private CommentManager() { }
        //public List<Comment> Comments { get; set; }

        public static CommentManager Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new CommentManager();
                }
                return instance;
            }
        }

        public Comment GetCommentById(int id)
        {
            using (UsersContext ct = new UsersContext())
            {
                Comment g = ct.Comments.Where(i => i.Id == id).FirstOrDefault();
                return g;
            }
        }

        public void AddCommentToDocument(string content, int documentId, int userId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Document g = ct.Documents.Find(documentId);
                if (g == null) return;

                ct.Comments.Add(new Comment
                {
                    Content = content,
                    createdDate = DateTime.Now,
                    DocumentId = documentId,
                    WrittenbyId = userId
                });

                ct.SaveChanges();
            }
        }

        public void DeleteComment(int Id)
        {
            using (UsersContext ct = new UsersContext())
            {
                Comment g = ct.Comments.Find(Id);
                if (g == null) return;
                ct.Comments.Remove(g);
                ct.SaveChanges();
            }
        }

        public IEnumerable<Comment> GetCommentsForDocument(int documentId)
        {
            using (UsersContext ct = new UsersContext())
            {
                Document g = ct.Documents.Find(documentId);
                if (g == null) return null;

                return ct.Comments.Where(i => i.DocumentId == documentId).OrderByDescending( i => i.createdDate).ToList();
            }
        }
    }
}