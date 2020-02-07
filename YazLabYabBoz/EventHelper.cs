using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YazLabYabBoz
{
    public class EventHelper
    {
        // Bu method form üezerindeki tüm buttonlara istenilen yeteneği kazandırır.
        public void SetUpDragDropForEachButton(Form form)
        {
            foreach (var button in form.Controls.OfType<Button>())
            {
                //bir butona event tanımlama alttaki şekilde yapoılır.
                button.MouseDown += new System.Windows.Forms.MouseEventHandler(button_MouseDown);
                button.DragEnter += new System.Windows.Forms.DragEventHandler(button_DragEnter);
                button.DragDrop += new System.Windows.Forms.DragEventHandler(button_DragDrop);
            }
        }

        private void button_MouseDown(object sender, EventArgs e)
        {
            var button = (Button)sender;
            Console.WriteLine("Called: button1_MouseDown");
            button.DoDragDrop(button, DragDropEffects.Move);
        }

        private void button_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("Called: button1_DragEnter");
            e.Effect = DragDropEffects.Move;
        }

        private void button_DragDrop(object sender, DragEventArgs e)
        {
            var gelen_buton = ((Button)e.Data.GetData(typeof(Button)));
            var evsahibi_buton = ((Button)sender);
            Console.WriteLine("Called: {0}_DragDrop", evsahibi_buton.Name);
            Console.WriteLine(gelen_buton.Name + " buttonu {0} içine sürüklendi", evsahibi_buton.Name);
            Point temp = gelen_buton.Location;
            gelen_buton.Location = evsahibi_buton.Location;
            evsahibi_buton.Location = temp;

           // PuzzleCheck();
        }
    }

}
