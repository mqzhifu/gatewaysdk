using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;


public class images : MonoBehaviour
{

    private Image image;

    List<Texture2D> allTex2d = new List<Texture2D>();
    string dirPrefix = "Assets/images/";
    // Start is called before the first frame update
    void Start()
    {
        //找到对象
        image = GameObject.Find("Image").GetComponent<Image>();
        //读取目录下的男版 
        var fileList = this.GetFileList(dirPrefix);
        //Debug.Log(fileList.Count);
        if (fileList.Count <= 0)
        {
            Debug.Log("err:list count == 0 ");
        }
        //遍历图片，把图片读入到组件中
        foreach (var element in fileList)
        {
            Debug.Log(element);

            Texture2D tx = new Texture2D(0, 0);
            tx.LoadImage(getImageByte(element));
            Debug.Log("tx width:"+tx.width + " , height:" + tx.height);

            //System.Drawing.Image originalImage = System.Drawing.Image.FromFile(originalImagePath);

            //image.sprite = Sprite.Create(tx, new Rect(0, 0, tx.width, tx.height), new Vector2(0.5f, 0.5f));
            //allTex2d.Add(tx);
            //Bitmap bitmap = new Bitmap(128, 160);

        }
    }
    //读取某个目录下的所有文件，并过滤掉无用的
    private List<string> GetFileList(string filePath)
    {
        DirectoryInfo di = new DirectoryInfo(filePath);
        FileInfo[] fileList = di.GetFiles("*.*");
        string fileName;
        List<string> list = new List<string>();

        for (int i = 0; i < fileList.Length; i++)
        {
            fileName = fileList[i].Name.ToLower();
            if (fileName.EndsWith(".jpeg") || fileName.EndsWith(".jpg") || fileName.EndsWith(".text") || fileName.EndsWith(".png"))
            {
                list.Add(dirPrefix + fileName);
            }
        }
        return list;
    }
    //读取文件流
    private static byte[] getImageByte(string imagePath)
    {
        FileStream files = new FileStream(imagePath, FileMode.Open);



        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;
    }


    //void Update()
    //{

    //}
}




