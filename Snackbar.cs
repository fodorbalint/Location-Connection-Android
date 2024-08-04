using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.ConstraintLayout.Widget;
using Org.W3c.Dom;

namespace LocationConnection;

/* Error when inflating layout in ListActivity:
 * 
 * System.NotSupportedException
  Message=Could not activate JNI Handle 0x7fda5fde20 (key_handle 0xee3bb0c) of Java type 'crc643157453c4ef3457f/Snackbar' as managed type 'LocationConnection.Snackbar'.*/

//[Register("locationconnection.Snackbar")]
public class Snackbar : ConstraintLayout
{
    TextView Message;
    Button OKButton;
    private string _message;
    private bool _showButton;
    private string _buttonText;
    BaseActivity context;

    public string message
    {
        get => message;
        set
        {
            _message = value;
            Message = FindViewById<TextView>(Resource.Id.Message);
            Message.Text = _message;

            Invalidate();
        }
    }

    public bool showButton
    {
        get => _showButton;
        set
        {
            _showButton = value;
            OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Visibility = _showButton ? ViewStates.Visible : ViewStates.Gone;

            Invalidate();
        }
    }

    public string buttonText
    {
        get => _buttonText;
        set
        {
            _buttonText = value;
            OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Text = _buttonText;

            Invalidate();
        }
    }

    public Snackbar(Context context) : base(context)
    {
        Initialize(context);
    }

    public Snackbar(Context context, IAttributeSet attrs) : base(context, attrs)
    {
        Initialize(context, attrs);
    }

    private void Initialize(Context context, IAttributeSet attrs = null)
    {
        this.context = (BaseActivity)context;

        if (attrs != null) // set in .xml
        {
            // Contains the values set for the styleable attributes you declared in your attrs.xml
            var array = context.ObtainStyledAttributes(attrs, Resource.Styleable.Snackbar, 0, 0);

            _message = array.GetString(Resource.Styleable.Snackbar[0]);
            _showButton = array.GetBoolean(Resource.Styleable.Snackbar[1], false);

            // Very important to recycle the array after use
            array.Recycle();

            LayoutInflater inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.snackbar, null, true);
            this.AddView(view);

            Message = FindViewById<TextView>(Resource.Id.Message);
            Message.Text = _message;

            OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Visibility = _showButton ? ViewStates.Visible : ViewStates.Gone;
        }
        else // added in .cs
        {
            LayoutInflater inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            View view = inflater.Inflate(Resource.Layout.snackbar, null, true);
            AddView(view);

            var p = new ConstraintLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);
            view.LayoutParameters = p;

            OKButton = FindViewById<Button>(Resource.Id.OKButton);
            OKButton.Click += OKButton_Click;
        }
    }

    private void OKButton_Click(object? sender, EventArgs e)
    {
        context.c.CloseSnack();
    }
}